using FluentValidation.TestHelper;
using Titan.Infrastructure.Endpoints.Admin;

namespace Titan.Tests.Infrastructure;

// The validator applies the aggregate's own rules, so a request it lets through is never refused later as a 500.
public class UpsertClusterValidatorTests
{
    private const string Fingerprint = "ffcf7803b2371e90834c2dd2a454f48bcb9c541b16dd7dc54a93a104b1aa108f";
    private readonly UpsertClusterValidator _validator = new();

    [Theory]
    [InlineData("https://incus-lab:8443", Fingerprint)]
    [InlineData("https://incus-lab:8443/", "FF:CF:78:03:B2:37:1E:90:83:4C:2D:D2:A4:54:F4:8B:CB:9C:54:1B:16:DD:7D:C5:4A:93:A1:04:B1:AA:10:8F")]
    public void AnHttpsEndpointAndASha256_AreAccepted(string endpoint, string fingerprint)
    {
        var result = _validator.TestValidate(new UpsertClusterRequest(null, endpoint, fingerprint));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AnHttpEndpoint_IsRefused_OnTheEndpoint()
    {
        var result = _validator.TestValidate(new UpsertClusterRequest(null, "http://incus-lab:8443", Fingerprint));

        result.ShouldHaveValidationErrorFor(r => r.Endpoint);
        result.ShouldNotHaveValidationErrorFor(r => r.ServerFingerprint);
    }

    [Fact]
    public void AShortFingerprint_IsRefused_OnTheFingerprint()
    {
        var result = _validator.TestValidate(new UpsertClusterRequest(null, "https://incus-lab:8443", "ffcf"));

        result.ShouldHaveValidationErrorFor(r => r.ServerFingerprint);
        result.ShouldNotHaveValidationErrorFor(r => r.Endpoint);
    }
}
