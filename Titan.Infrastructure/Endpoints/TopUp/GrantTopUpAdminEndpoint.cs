using Titan.Infrastructure.Auth;
using FastEndpoints;
using FluentValidation;
using Titan.Application.Commands.Billing.TopUp;
using Titan.Application.Commands.Billing.TopUp.Contracts;

namespace Titan.Infrastructure.Endpoints.TopUp;

// A distinct sub-action, not POST /topups with a source param, so the free-money grant stays auditably
// separate from a real-money confirm. The handler enforces the config cap (GRANT_AMOUNT_EXCEEDS_MAX).
public class GrantTopUpValidator : Validator<GrantTopUpCommand>
{
    public GrantTopUpValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).MaximumLength(500).When(x => x.Reason is not null);
    }
}

public class GrantTopUpAdminEndpoint(GrantTopUpCommandHandler handler)
    : Endpoint<GrantTopUpCommand, GrantTopUpCommandResult>, IRequiresIdempotencyKey
{
    public override void Configure()
    {
        Post("v1/admin/topups/grant");
        Roles("platform-admin");
    }

    public override async Task HandleAsync(GrantTopUpCommand command, CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);
        await SendAsync(result, 201, ct);
    }
}
