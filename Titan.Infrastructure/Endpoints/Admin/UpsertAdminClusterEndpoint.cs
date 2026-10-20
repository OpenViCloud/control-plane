using FastEndpoints;
using FluentValidation;
using Titan.Application.Abstractions;

namespace Titan.Infrastructure.Endpoints.Admin;

public record UpsertClusterRequest(string? Name, string Endpoint, string ServerFingerprint);

// PUT is an upsert keyed by {zoneId} (https://github.com/OpenViCloud/titan/blob/main/docs/features/cluster/design.md#cluster-incus).
public class UpsertAdminClusterEndpoint(IAdminClusterService clusters)
    : Endpoint<UpsertClusterRequest>
{
    public override void Configure()
    {
        Put("v1/admin/clusters/{zoneId}");
        Roles("platform-admin");
    }

    public override async Task HandleAsync(UpsertClusterRequest req, CancellationToken ct)
    {
        var zoneId = Route<string>("zoneId")!;
        await clusters.UpsertAsync(zoneId, req.Name, req.Endpoint, req.ServerFingerprint, ct);
        await SendNoContentAsync(ct);
    }
}

public class UpsertClusterValidator : Validator<UpsertClusterRequest>
{
    public UpsertClusterValidator()
    {
        RuleFor(x => x.Endpoint).Must(ClusterConnectionRules.IsValidEndpoint)
            .WithMessage("endpoint must be an https URL");
        RuleFor(x => x.ServerFingerprint).Must(ClusterConnectionRules.IsValidFingerprint)
            .WithMessage("serverFingerprint must be a SHA-256 fingerprint (64 hex digits)");
    }
}
