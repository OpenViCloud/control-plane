using Microsoft.AspNetCore.Http;
using Titan.Infrastructure.Auth;
using FastEndpoints;
using FluentValidation;
using Titan.Application.Commands.Billing.TopUp;
using Titan.Application.Commands.Billing.TopUp.Contracts;

namespace Titan.Infrastructure.Endpoints.TopUp;

// Only starts a Pending TopUp with a PaymentRef to pay against; confirmation is a separate step (an admin
// or the SePay webhook).
public class RequestTopUpValidator : Validator<RequestTopUpCommand>
{
    public RequestTopUpValidator() =>
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Top-up amount must be greater than 0.");
}

public class CreateTopUpEndpoint(RequestTopUpCommandHandler handler)
    : Endpoint<RequestTopUpCommand, RequestTopUpCommandResult>, IRequiresIdempotencyKey
{
    public override void Configure()
    {
        Post("v1/tenant/topups");
        Description(b => b.ClearDefaultProduces(200).Produces<RequestTopUpCommandResult>(201, "application/json"));
    }

    public override async Task HandleAsync(RequestTopUpCommand command, CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);
        await SendAsync(result, 201, ct);
    }
}
