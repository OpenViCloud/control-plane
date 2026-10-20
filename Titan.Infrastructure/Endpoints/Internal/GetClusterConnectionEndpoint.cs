using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Titan.Domain.Exceptions;
using Titan.Domain.Repositories.Platform;
using Titan.Infrastructure.Incus;
using ClusterAggregate = Titan.Domain.Aggregates.Platform.Cluster.Cluster;

namespace Titan.Infrastructure.Endpoints.Internal;

// The worker's way to a zone's Incus cluster (https://github.com/OpenViCloud/titan/blob/main/docs/features/cluster/design.md#worker-cluster-connection): endpoint,
// the server fingerprint to pin, Titan's client certificate WITH its private key, and the zone's provider settings. Behind X-Internal-Token only;
// the body is never logged.
public class GetClusterConnectionEndpoint(
    IClusterRepository clusters, ProviderClientIdentityStore identity, IConfiguration config)
    : EndpointWithoutRequest<ClusterConnectionResponse>
{
    public override void Configure()
    {
        Get("internal/clusters/{zoneId}/connection");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string? token = HttpContext.Request.Headers["X-Internal-Token"].FirstOrDefault();
        bool callerIsTheWorker = InternalTokenAuth.Matches(token, config["Internal:Token"]);
        if (!callerIsTheWorker)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        string zoneId = Route<string>("zoneId") ?? "";
        ClusterAggregate? cluster = await clusters.FindByZoneIdAsync(zoneId, ct);
        if (cluster == null)
        {
            throw new NotFoundException("CLUSTER_NOT_FOUND", $"Zone '{zoneId}' has no cluster.");
        }
        ProviderClientKeyPair keyPair = await identity.GetKeyPairAsync(ct);
        await SendAsync(
            new ClusterConnectionResponse(cluster.Endpoint, cluster.ServerFingerprint, keyPair.CertificatePem, keyPair.KeyPem,
                cluster.ProviderSettings.OvnUplinkNetwork, cluster.ProviderSettings.StoragePool),
            cancellation: ct);
    }
}

public record ClusterConnectionResponse(
    string Endpoint,
    string ServerFingerprint,
    string ClientCertificatePem,
    string ClientKeyPem,
    // The zone's provider settings (https://github.com/OpenViCloud/titan/blob/main/docs/features/cluster/design.md#cluster-provider-settings); null = not set.
    string? OvnUplinkNetwork,
    string? StoragePool);
