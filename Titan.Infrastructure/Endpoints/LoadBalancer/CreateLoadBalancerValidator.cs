using FastEndpoints;
using FluentValidation;
using Titan.Application.Commands.Compute.LoadBalancer.Contracts;

namespace Titan.Infrastructure.Endpoints.LoadBalancer;

public class CreateLoadBalancerValidator : Validator<CreateLoadBalancerCommand>
{
    private static readonly System.Text.RegularExpressions.Regex NameRegex =
        new(@"^[a-zA-Z0-9][a-zA-Z0-9_-]*$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public CreateLoadBalancerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
            .Matches(NameRegex).WithMessage("'Name' may only contain letters, digits, hyphens, and underscores, and must start with a letter or digit.");
        RuleFor(x => x.NetworkId).NotEmpty();
        RuleFor(x => x.Listeners).Must(LoadBalancerSpecRules.ListenersAreValid).WithMessage(LoadBalancerSpecRules.ListenersMessage);
        RuleFor(x => x.Backends).Must(LoadBalancerSpecRules.BackendsAreValid).WithMessage(LoadBalancerSpecRules.BackendsMessage);
    }
}
