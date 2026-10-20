using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Titan.Application.Commands.Networking.Network;
using Titan.Application.Commands.Networking.Network.Contracts;

namespace Titan.Infrastructure.Endpoints.Internal;

public class UpdateNetworkStatusEndpoint(UpdateNetworkStatusCommandHandler handler, IConfiguration config)
    : Endpoint<UpdateNetworkStatusRequest>
{
    public override void Configure()
    {
        Patch("internal/networks/{networkId:guid}/status");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateNetworkStatusRequest req, CancellationToken ct)
    {
        var token = HttpContext.Request.Headers["X-Internal-Token"].FirstOrDefault();
        if (!InternalTokenAuth.Matches(token, config["Internal:Token"]))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var networkId = Route<Guid>("networkId");
        await handler.HandleAsync(
            new UpdateNetworkStatusCommand(networkId, req.Report, req.Exists, req.Status, req.ExternalId, req.Cidr,
                req.Gateway, req.Reason, req.Detail),
            ct);
        await SendNoContentAsync(ct);
    }
}

// The worker's JSON body; UpdateNetworkStatusCommand additionally carries the route id.
public record UpdateNetworkStatusRequest(
    string Report,
    bool? Exists = null,
    string? Status = null,
    string? ExternalId = null,
    string? Cidr = null,
    string? Gateway = null,
    string? Reason = null,
    string? Detail = null);
