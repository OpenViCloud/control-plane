using FastEndpoints;
using FluentValidation;

namespace Titan.Infrastructure.Endpoints.Volume;

public class AttachVolumeValidator : Validator<AttachVolumeBody>
{
    public AttachVolumeValidator()
    {
        RuleFor(x => x.VmId).NotEmpty();
    }
}
