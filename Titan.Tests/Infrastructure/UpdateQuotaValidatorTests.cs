using FluentValidation.TestHelper;
using Titan.Infrastructure.Endpoints.Tenant;

namespace Titan.Tests.Infrastructure;

// A cap the admin sends must be non-negative; a cap the admin leaves out is kept by the handler, so absent is valid.
public class UpdateQuotaValidatorTests
{
    private readonly UpdateQuotaValidator _validator = new();

    private static UpdateQuotaRequest AllCaps(int value) =>
        new(value, value, value, value, value, value, value, value, value);

    [Fact]
    public void ZeroOnEveryCap_IsAccepted()
    {
        _validator.TestValidate(AllCaps(0)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void OnlyTheTwoRequiredCaps_IsAccepted()
    {
        var request = new UpdateQuotaRequest(5, 1, null, null, null, null, null, null, null);

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ANegativeValue_IsRejected_OnEachCap()
    {
        var result = _validator.TestValidate(AllCaps(-1));

        result.ShouldHaveValidationErrorFor(r => r.MaxVms);
        result.ShouldHaveValidationErrorFor(r => r.MaxNetworks);
        result.ShouldHaveValidationErrorFor(r => r.MaxVolumes);
        result.ShouldHaveValidationErrorFor(r => r.MaxVmSnapshotsPerVm);
        result.ShouldHaveValidationErrorFor(r => r.MaxVolumeSnapshotsPerVolume);
        result.ShouldHaveValidationErrorFor(r => r.MaxFirewallRulesPerNetwork);
        result.ShouldHaveValidationErrorFor(r => r.MaxPublicIpv6);
        result.ShouldHaveValidationErrorFor(r => r.MaxSshKeys);
        result.ShouldHaveValidationErrorFor(r => r.MaxLoadBalancers);
    }

    // An int field the JSON leaves out binds to 0, and 0 is a valid cap: a body without maxVms would silently close the
    // tenant to new VMs. The two caps every client sends are therefore required, not defaulted.
    [Fact]
    public void AMissingRequiredCap_IsRejected()
    {
        var request = new UpdateQuotaRequest(null, null, 5, null, null, null, null, null, null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.MaxVms);
        result.ShouldHaveValidationErrorFor(r => r.MaxNetworks);
    }
}
