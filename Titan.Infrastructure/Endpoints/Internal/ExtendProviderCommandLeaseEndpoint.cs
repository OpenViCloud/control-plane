using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Titan.Application.Commands.Platform.ProviderCommand;
using Titan.Application.Commands.Platform.ProviderCommand.Contracts;

namespace Titan.Infrastructure.Endpoints.Internal;

// 409 PROVIDER_COMMAND_LEASE_LOST: another lease took the command; the worker stops working on it.
public class ExtendProviderCommandLeaseEndpoint(ExtendProviderCommandLeaseCommandHandler handler, IConfiguration config)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Patch("internal/provider-commands/leases/{leaseId:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string? token = HttpContext.Request.Headers["X-Internal-Token"].FirstOrDefault();
        bool callerIsTheWorker = InternalTokenAuth.Matches(token, config["Internal:Token"]);
        if (!callerIsTheWorker)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        Guid leaseId = Route<Guid>("leaseId");
        await handler.HandleAsync(new ExtendProviderCommandLeaseCommand(leaseId), ct);
        await SendNoContentAsync(ct);
    }
}
