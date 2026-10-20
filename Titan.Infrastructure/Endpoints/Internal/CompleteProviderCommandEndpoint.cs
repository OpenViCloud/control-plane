using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Titan.Application.Commands.Platform.ProviderCommand;
using Titan.Application.Commands.Platform.ProviderCommand.Contracts;

namespace Titan.Infrastructure.Endpoints.Internal;

public class CompleteProviderCommandEndpoint(CompleteProviderCommandCommandHandler handler, IConfiguration config)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Put("internal/provider-commands/leases/{leaseId:guid}/completion");
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
        await handler.HandleAsync(new CompleteProviderCommandCommand(leaseId), ct);
        await SendNoContentAsync(ct);
    }
}
