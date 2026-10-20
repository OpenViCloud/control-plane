using FastEndpoints;
using FluentValidation;

namespace Titan.Infrastructure.Endpoints.VmSnapshots;

public class CreateSnapshotValidator : Validator<CreateSnapshotBody>
{
    public CreateSnapshotValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
