using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;

namespace Titan.Tests.Infrastructure.Incus;

// An in-process HTTPS server that answers like the lab Incus: any client certificate passes the handshake, trust is
// decided per request by the certificate's SHA-256 fingerprint, and bodies come from IncusLabRecording.
internal sealed class FakeIncusServer : IAsyncDisposable
{
    private readonly WebApplication _app;

    public string Endpoint { get; }
    public string Fingerprint { get; }
    public HashSet<string> TrustedClientFingerprints { get; }
    public Dictionary<string, (int Status, string Body)> TrustedOverrides { get; } = new();

    private FakeIncusServer(WebApplication app, string endpoint, string fingerprint, HashSet<string> trusted)
    {
        _app = app;
        Endpoint = endpoint;
        Fingerprint = fingerprint;
        TrustedClientFingerprints = trusted;
    }

    public static async Task<FakeIncusServer> StartAsync(params string[] trustedClientFingerprints)
    {
        X509Certificate2 serverCert = TestTlsCertificates.Server();
        var trusted = new HashSet<string>(trustedClientFingerprints);
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.ConfigureKestrel(kestrel =>
            kestrel.Listen(IPAddress.Loopback, 0, listen => listen.UseHttps(new HttpsConnectionAdapterOptions
            {
                ServerCertificate = serverCert,
                ClientCertificateMode = ClientCertificateMode.AllowCertificate,
                ClientCertificateValidation = (_, _, _) => true,
            })));
        WebApplication app = builder.Build();
        FakeIncusServer? server = null;
        app.Run(async context =>
        {
            X509Certificate2? client = context.Connection.ClientCertificate;
            bool isTrusted = client != null
                && trusted.Contains(Convert.ToHexStringLower(SHA256.HashData(client.RawData)));
            string pathAndQuery = context.Request.Path + context.Request.QueryString;
            (int status, string body) = IncusLabRecording.Answer(isTrusted ? "trusted" : "untrusted", pathAndQuery);
            bool overridden = isTrusted && server!.TrustedOverrides.TryGetValue(pathAndQuery, out var custom);
            if (overridden)
            {
                (status, body) = server!.TrustedOverrides[pathAndQuery];
            }
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(body);
        });
        await app.StartAsync();
        string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        server = new FakeIncusServer(app, address, TestTlsCertificates.Fingerprint(serverCert), trusted);
        return server;
    }

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
