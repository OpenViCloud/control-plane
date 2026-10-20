using FastEndpoints;
using FluentValidation;

namespace Titan.Infrastructure.Endpoints.VolumeSnapshots;

public class CreateVolumeSnapshotValidator : Validator<CreateVolumeSnapshotBody>
{
    public CreateVolumeSnapshotValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
