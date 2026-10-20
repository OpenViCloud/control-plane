using FastEndpoints;
using FluentValidation;
using Titan.Application.Commands.Platform.Tenant;
using Titan.Application.Commands.Platform.Tenant.Contracts;

namespace Titan.Infrastructure.Endpoints.Tenant;

// An absent optional cap is kept as it is (UpdateTenantQuotaCommand). MaxVms and MaxNetworks are nullable only so an
// absent one is told apart from 0 and refused (an int would bind a missing field to 0, a valid cap).
public record UpdateQuotaRequest(
    int? MaxVms,
    int? MaxNetworks,
    int? MaxVolumes,
    int? MaxVmSnapshotsPerVm,
    int? MaxVolumeSnapshotsPerVolume,
    int? MaxFirewallRulesPerNetwork,
    int? MaxPublicIpv6,
    int? MaxSshKeys,
    int? MaxLoadBalancers);

// Covered by TenantServiceTests.
public class UpdateTenantQuotaEndpoint(UpdateTenantQuotaCommandHandler updateQuota)
    : Endpoint<UpdateQuotaRequest>
{
    public override void Configure()
    {
        Patch("v1/tenants/{tenantId:guid}/quota");
        Roles("platform-admin");
    }

    public override async Task HandleAsync(UpdateQuotaRequest req, CancellationToken ct)
    {
        var tenantId = Route<Guid>("tenantId");
        int? maxVms = req.MaxVms;
        int? maxNetworks = req.MaxNetworks;
        if (!maxVms.HasValue || !maxNetworks.HasValue)
        {
            throw new InvalidOperationException("UpdateQuotaValidator lets no request without maxVms and maxNetworks through.");
        }

        var command = new UpdateTenantQuotaCommand(
            tenantId, maxVms.Value, maxNetworks.Value, req.MaxVolumes, req.MaxVmSnapshotsPerVm,
            req.MaxVolumeSnapshotsPerVolume, req.MaxFirewallRulesPerNetwork, req.MaxPublicIpv6, req.MaxSshKeys,
            req.MaxLoadBalancers);
        await updateQuota.HandleAsync(command, ct);
        await SendNoContentAsync(ct);
    }
}

public class UpdateQuotaValidator : Validator<UpdateQuotaRequest>
{
    public UpdateQuotaValidator()
    {
        RuleFor(x => x.MaxVms).NotNull().WithMessage("maxVms is required");
        RuleFor(x => x.MaxVms).GreaterThanOrEqualTo(0).WithMessage("maxVms must be non-negative");
        RuleFor(x => x.MaxNetworks).NotNull().WithMessage("maxNetworks is required");
        RuleFor(x => x.MaxNetworks).GreaterThanOrEqualTo(0).WithMessage("maxNetworks must be non-negative");
        RuleFor(x => x.MaxVolumes).GreaterThanOrEqualTo(0).WithMessage("maxVolumes must be non-negative");
        RuleFor(x => x.MaxVmSnapshotsPerVm).GreaterThanOrEqualTo(0)
            .WithMessage("maxVmSnapshotsPerVm must be non-negative");
        RuleFor(x => x.MaxVolumeSnapshotsPerVolume).GreaterThanOrEqualTo(0)
            .WithMessage("maxVolumeSnapshotsPerVolume must be non-negative");
        RuleFor(x => x.MaxFirewallRulesPerNetwork).GreaterThanOrEqualTo(0)
            .WithMessage("maxFirewallRulesPerNetwork must be non-negative");
        RuleFor(x => x.MaxPublicIpv6).GreaterThanOrEqualTo(0).WithMessage("maxPublicIpv6 must be non-negative");
        RuleFor(x => x.MaxSshKeys).GreaterThanOrEqualTo(0).WithMessage("maxSshKeys must be non-negative");
        RuleFor(x => x.MaxLoadBalancers).GreaterThanOrEqualTo(0).WithMessage("maxLoadBalancers must be non-negative");
    }
}
