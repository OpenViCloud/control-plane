using FastEndpoints;
using FluentValidation;

namespace Titan.Infrastructure.Endpoints.Network;

public class RenameNetworkValidator : Validator<RenameNetworkRequest>
{
    private static readonly System.Text.RegularExpressions.Regex NameRegex =
        new(@"^[a-zA-Z0-9][a-zA-Z0-9_-]*$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public RenameNetworkValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
            .Matches(NameRegex).WithMessage("'Name' may only contain letters, digits, hyphens, and underscores, and must start with a letter or digit.");
    }
}
