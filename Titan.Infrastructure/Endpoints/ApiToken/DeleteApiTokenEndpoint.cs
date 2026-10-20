using Microsoft.AspNetCore.Http;
using FastEndpoints;
using Titan.Application.Commands.Platform.ApiToken;
using Titan.Application.Commands.Platform.ApiToken.Contracts;

namespace Titan.Infrastructure.Endpoints.ApiToken;

public class DeleteApiTokenEndpoint(DeleteApiTokenCommandHandler handler) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("v1/tenant/api-tokens/{apiTokenId:guid}");
        Description(b => b.ClearDefaultProduces(200).Produces(204));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var apiTokenId = Route<Guid>("apiTokenId");
        await handler.HandleAsync(new DeleteApiTokenCommand(apiTokenId), ct);
        await SendNoContentAsync(ct);
    }
}
