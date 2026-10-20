using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Titan.Tests.Infrastructure.Incus;

internal static class TestTlsCertificates
{
    // Shaped like the lab server certificate: ECDSA P-384, server auth, the DNS name only — no IP, so a client that
    // dials 127.0.0.1 gets past only through the pinned fingerprint.
    public static X509Certificate2 Server(string dnsName = "incus-lab")
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        var request = new CertificateRequest("CN=root@" + dnsName + ", O=Linux Containers", key, HashAlgorithmName.SHA384);
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid("1.3.6.1.5.5.7.3.1")], false));
        var san = new SubjectAlternativeNameBuilder();
        san.AddDnsName(dnsName);
        request.CertificateExtensions.Add(san.Build());
        using var cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow.AddDays(1));
        // Round-trip through PKCS#12: macOS refuses an ephemeral key for a TLS server.
        return X509CertificateLoader.LoadPkcs12(cert.Export(X509ContentType.Pkcs12), null);
    }

    public static string Fingerprint(X509Certificate2 cert) => Convert.ToHexStringLower(SHA256.HashData(cert.RawData));
}
