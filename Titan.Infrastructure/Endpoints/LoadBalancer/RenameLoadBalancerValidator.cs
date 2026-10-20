using FastEndpoints;
using FluentValidation;

namespace Titan.Infrastructure.Endpoints.LoadBalancer;

// The same name rule as create (CreateLoadBalancerValidator).
public class RenameLoadBalancerValidator : Validator<RenameLoadBalancerBody>
{
    private static readonly System.Text.RegularExpressions.Regex NameRegex =
        new(@"^[a-zA-Z0-9][a-zA-Z0-9_-]*$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public RenameLoadBalancerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
            .Matches(NameRegex).WithMessage("'Name' may only contain letters, digits, hyphens, and underscores, and must start with a letter or digit.");
    }
}
