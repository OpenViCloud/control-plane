using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Titan.Application.Queries.Platform.ProviderCommand;
using Titan.Application.Queries.Platform.ProviderCommand.Contracts;

namespace Titan.Infrastructure.Endpoints.Internal;

// 404 PROVIDER_COMMAND_LEASE_NOT_FOUND is the normal "no work was due" answer.
public class GetLeasedProviderCommandEndpoint(GetLeasedProviderCommandQueryHandler handler, IConfiguration config)
    : EndpointWithoutRequest<LeasedProviderCommandResult>
{
    public override void Configure()
    {
        Get("internal/provider-commands/leases/{leaseId:guid}");
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
        LeasedProviderCommandResult leased = await handler.HandleAsync(new GetLeasedProviderCommandQuery(leaseId), ct);
        await SendAsync(leased, cancellation: ct);
    }
}
