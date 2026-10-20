using FastEndpoints;
using FluentValidation;

namespace Titan.Infrastructure.Endpoints.Vm;

public class UpdateVmValidator : Validator<RenameVmBody>
{
    private static readonly System.Text.RegularExpressions.Regex NameRegex =
        new(@"^[a-zA-Z0-9][a-zA-Z0-9-]*$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public UpdateVmValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(64)
            .Matches(NameRegex).WithMessage("'Name' may only contain letters, digits, and hyphens, and must start with a letter or digit.");
    }
}
