using Microsoft.AspNetCore.Http;
using Titan.Infrastructure.Auth;
using FastEndpoints;
using Titan.Application.Commands.Networking.Network;
using Titan.Application.Commands.Networking.Network.Contracts;

namespace Titan.Infrastructure.Endpoints.Network;

public class CreateNetworkEndpoint(CreateNetworkCommandHandler handler) : Endpoint<CreateNetworkCommand, CreateNetworkCommandResult>, IRequiresIdempotencyKey
{
    public override void Configure()
    {
        Post("v1/tenant/networks");
        Description(b => b.ClearDefaultProduces(200).Produces<CreateNetworkCommandResult>(202, "application/json"));
    }

    public override async Task HandleAsync(CreateNetworkCommand req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendAsync(result, 202, ct);
    }
}
