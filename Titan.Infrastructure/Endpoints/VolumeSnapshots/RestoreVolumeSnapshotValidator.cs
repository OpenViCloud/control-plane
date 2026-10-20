using FastEndpoints;
using FluentValidation;

namespace Titan.Infrastructure.Endpoints.VolumeSnapshots;

public class RestoreVolumeSnapshotValidator : Validator<RestoreVolumeSnapshotBody>
{
    public RestoreVolumeSnapshotValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
