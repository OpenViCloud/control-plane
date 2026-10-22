using FastEndpoints;
using FluentValidation;
using Titan.Application.Abstractions;
using Titan.Domain.Aggregates.Platform.Cluster;

namespace Titan.Infrastructure.Endpoints.Admin;

// Every field nullable: null = not set (https://github.com/OpenViCloud/titan-ops/blob/main/docs/features/cluster/design.md#cluster-provider-settings).
public record SetClusterProviderSettingsRequest(string? OvnUplinkNetwork, string? StoragePool, string? Ipv6Block);

// Its own route, not a field of the connection PUT: the admin page saves the connection without these, and an
// omitted field there would have to mean "keep" while here it means "not set".
public class SetAdminClusterProviderSettingsEndpoint(IAdminClusterService clusters)
    : Endpoint<SetClusterProviderSettingsRequest>
{
    public override void Configure()
    {
        Put("v1/admin/clusters/{zoneId}/provider-settings");
        Roles("platform-admin");
    }

    public override async Task HandleAsync(SetClusterProviderSettingsRequest req, CancellationToken ct)
    {
        var zoneId = Route<string>("zoneId")!;
        var settings = ClusterProviderSettings.Create(req.OvnUplinkNetwork, req.StoragePool, req.Ipv6Block);
        await clusters.SetProviderSettingsAsync(zoneId, settings, ct);
        await SendNoContentAsync(ct);
    }
}

public class SetClusterProviderSettingsValidator : Validator<SetClusterProviderSettingsRequest>
{
    public SetClusterProviderSettingsValidator()
    {
        RuleFor(x => x.OvnUplinkNetwork).MaximumLength(64);
        RuleFor(x => x.StoragePool).MaximumLength(64);
        RuleFor(x => x.Ipv6Block)
            .Must(block => string.IsNullOrWhiteSpace(block) || ClusterProviderSettings.IsIpv6Block(block.Trim()))
            .WithMessage("'Ipv6 Block' must be an IPv6 /56 written as its first address (e.g. 2001:db8:0:100::/56).");
    }
}
