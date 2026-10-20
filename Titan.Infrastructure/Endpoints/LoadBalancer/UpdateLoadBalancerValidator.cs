using FastEndpoints;
using FluentValidation;

namespace Titan.Infrastructure.Endpoints.LoadBalancer;

public class UpdateLoadBalancerValidator : Validator<UpdateLoadBalancerBody>
{
    public UpdateLoadBalancerValidator()
    {
        RuleFor(x => x.Listeners).Must(LoadBalancerSpecRules.ListenersAreValid).WithMessage(LoadBalancerSpecRules.ListenersMessage);
        RuleFor(x => x.Backends).Must(LoadBalancerSpecRules.BackendsAreValid).WithMessage(LoadBalancerSpecRules.BackendsMessage);
    }
}
