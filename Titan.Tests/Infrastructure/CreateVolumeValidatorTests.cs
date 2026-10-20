using FluentValidation.TestHelper;
using Titan.Application.Commands.Compute.Volume.Contracts;
using Titan.Infrastructure.Endpoints.Volume;

namespace Titan.Tests.Infrastructure;

// SizeGb > 0 must hold unconditionally: a `.When(x => x.SizeGb > 0)` gate on the same rule would make it
// never fire, and a volume of 0 or negative size would pass validation.
public class CreateVolumeValidatorTests
{
    private readonly CreateVolumeValidator _validator = new();

    private static CreateVolumeCommand Request(int sizeGb) =>
        new("vol-1", "standard", "vn-hcm-1", sizeGb);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void NonPositiveSizeGb_IsRejected(int sizeGb)
    {
        var result = _validator.TestValidate(Request(sizeGb));

        result.ShouldHaveValidationErrorFor(r => r.SizeGb);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public void PositiveSizeGb_IsAccepted(int sizeGb)
    {
        var result = _validator.TestValidate(Request(sizeGb));

        result.ShouldNotHaveValidationErrorFor(r => r.SizeGb);
    }
}
