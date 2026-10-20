using FastEndpoints;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Titan.Application.Commands.Platform.ProviderCommand;
using Titan.Application.Commands.Platform.ProviderCommand.Contracts;

namespace Titan.Infrastructure.Endpoints.Internal;

public class FailProviderCommandEndpoint(FailProviderCommandCommandHandler handler, IConfiguration config)
    : Endpoint<FailProviderCommandBody>
{
    public override void Configure()
    {
        Put("internal/provider-commands/leases/{leaseId:guid}/failure");
        AllowAnonymous();
    }

    public override async Task HandleAsync(FailProviderCommandBody body, CancellationToken ct)
    {
        string? token = HttpContext.Request.Headers["X-Internal-Token"].FirstOrDefault();
        bool callerIsTheWorker = InternalTokenAuth.Matches(token, config["Internal:Token"]);
        if (!callerIsTheWorker)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        Guid leaseId = Route<Guid>("leaseId");
        await handler.HandleAsync(new FailProviderCommandCommand(leaseId, body.Error, body.Retryable), ct);
        await SendNoContentAsync(ct);
    }
}

// FailProviderCommandCommand also carries the route's lease id.
public record FailProviderCommandBody(string Error, bool Retryable);

public class FailProviderCommandValidator : Validator<FailProviderCommandBody>
{
    public FailProviderCommandValidator()
    {
        RuleFor(b => b.Error).NotEmpty().MaximumLength(4096);
    }
}
