using FluentValidation.TestHelper;
using Titan.Infrastructure.Endpoints.Admin;

namespace Titan.Tests.Infrastructure;

// Null is valid on every field: it means "not set".
public class SetClusterProviderSettingsValidatorTests
{
    private readonly SetClusterProviderSettingsValidator _validator = new();

    [Fact]
    public void AllNull_IsAccepted()
    {
        var result = _validator.TestValidate(new SetClusterProviderSettingsRequest(null, null, null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Names_AreAccepted()
    {
        var result = _validator.TestValidate(new SetClusterProviderSettingsRequest("ovnup", "default", "2001:41d0:d00:d400::/56"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void OverlongNames_AreRejected()
    {
        string tooLong = new('n', 65);

        var result = _validator.TestValidate(new SetClusterProviderSettingsRequest(tooLong, tooLong, null));

        result.ShouldHaveValidationErrorFor(r => r.OvnUplinkNetwork);
        result.ShouldHaveValidationErrorFor(r => r.StoragePool);
    }

    [Theory]
    [InlineData("10.0.0.0/8")]
    [InlineData("2001:41d0:d00:d400::/64")]
    [InlineData("2001:41d0:d00:d401::/56")]
    public void Ipv6Block_NotAnIpv6Slash56_IsRejected(string block)
    {
        var result = _validator.TestValidate(new SetClusterProviderSettingsRequest("ovnup", "default", block));

        result.ShouldHaveValidationErrorFor(r => r.Ipv6Block);
    }
}
