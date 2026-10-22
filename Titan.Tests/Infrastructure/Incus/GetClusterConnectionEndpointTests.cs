using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Titan.Domain.Exceptions;
using Titan.Infrastructure.Endpoints.Internal;
using Titan.Infrastructure.Incus;
using ClusterAggregate = Titan.Domain.Aggregates.Platform.Cluster.Cluster;

namespace Titan.Tests.Infrastructure.Incus;

// https://github.com/OpenViCloud/titan-ops/blob/main/docs/features/cluster/design.md#worker-cluster-connection — the one route that hands Titan's private key out. It
// composes the zone's cluster with the client identity, so it carries its own logic and gets its own test.
public class GetClusterConnectionEndpointTests
{
    private const string Fingerprint = "ffcf78030000000000000000000000000000000000000000000000000000108f";

    private static IConfiguration ConfigWithToken(string token) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Internal:Token"] = token })
            .Build();

    private static GetClusterConnectionEndpoint EndpointFor(StubClusterRepository clusters, string sentToken) =>
        Factory.Create<GetClusterConnectionEndpoint>(
            ctx =>
            {
                ctx.Request.Headers["X-Internal-Token"] = sentToken;
                ctx.Request.RouteValues["zoneId"] = "mock";
            },
            clusters,
            IncusTestKit.IdentityStore(),
            ConfigWithToken("expected"));

    private static StubClusterRepository WithMockCluster()
    {
        var clusters = new StubClusterRepository();
        ClusterAggregate cluster = ClusterAggregate.Create("mock", "incus-lab", "https://incus-lab:8443", Fingerprint);
        cluster.SetProviderSettings(Titan.Domain.Aggregates.Platform.Cluster.ClusterProviderSettings.Create("UPLINK", "default", null));
        clusters.Rows.Add(cluster);
        return clusters;
    }

    [Fact]
    public async Task TheWorker_GetsTheEndpoint_ThePinnedFingerprint_AKeyPairThatMatches_AndTheZonesSettings()
    {
        GetClusterConnectionEndpoint endpoint = EndpointFor(WithMockCluster(), "expected");

        await endpoint.HandleAsync(CancellationToken.None);

        ClusterConnectionResponse body = endpoint.Response;
        Assert.Equal("https://incus-lab:8443", body.Endpoint);
        Assert.Equal(Fingerprint, body.ServerFingerprint);
        Assert.Equal("UPLINK", body.OvnUplinkNetwork);
        Assert.Equal("default", body.StoragePool);
        Assert.Contains("BEGIN CERTIFICATE", body.ClientCertificatePem);
        Assert.Contains("PRIVATE KEY", body.ClientKeyPem);
        using var pair = System.Security.Cryptography.X509Certificates.X509Certificate2.CreateFromPem(
            body.ClientCertificatePem, body.ClientKeyPem);
        Assert.True(pair.HasPrivateKey);
    }

    [Fact]
    public async Task WithoutTheInternalToken_Is401_AndNoKeyLeaves()
    {
        GetClusterConnectionEndpoint endpoint = EndpointFor(WithMockCluster(), "wrong");

        await endpoint.HandleAsync(CancellationToken.None);

        Assert.Equal(StatusCodes.Status401Unauthorized, endpoint.HttpContext.Response.StatusCode);
        // FastEndpoints pre-builds an empty Response object; what matters is that no key was put in it.
        bool keyLeft = endpoint.Response != null && !string.IsNullOrEmpty(endpoint.Response.ClientKeyPem);
        Assert.False(keyLeft);
    }

    // Encryption__ClusterKey changed after the key was stored: a named 503, not a 500 and not a garbled key.
    [Fact]
    public async Task AKeyThatNoLongerDecrypts_IsClusterIdentityUnreadable()
    {
        var identities = new StubIdentityRepository();
        await IncusTestKit.IdentityStore(identities).GetCertificateAsync(CancellationToken.None);
        var otherKeyStore = new ProviderClientIdentityStore(identities, IncusTestKit.Cipher(keySeed: 7), new ProviderClientCertificateCache());
        GetClusterConnectionEndpoint endpoint = Factory.Create<GetClusterConnectionEndpoint>(
            ctx =>
            {
                ctx.Request.Headers["X-Internal-Token"] = "expected";
                ctx.Request.RouteValues["zoneId"] = "mock";
            },
            WithMockCluster(),
            otherKeyStore,
            ConfigWithToken("expected"));

        ServiceUnavailableException ex = await Assert.ThrowsAsync<ServiceUnavailableException>(
            () => endpoint.HandleAsync(CancellationToken.None));
        Assert.Equal("CLUSTER_IDENTITY_UNREADABLE", ex.Code);
    }

    [Fact]
    public async Task AZoneWithoutACluster_IsClusterNotFound()
    {
        GetClusterConnectionEndpoint endpoint = EndpointFor(new StubClusterRepository(), "expected");

        NotFoundException ex = await Assert.ThrowsAsync<NotFoundException>(() => endpoint.HandleAsync(CancellationToken.None));
        Assert.Equal("CLUSTER_NOT_FOUND", ex.Code);
    }
}
