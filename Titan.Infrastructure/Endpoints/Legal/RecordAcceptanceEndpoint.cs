using Microsoft.AspNetCore.Http;
using FastEndpoints;
using Titan.Application.Commands.Platform.Legal;
using Titan.Application.Commands.Platform.Legal.Contracts;

namespace Titan.Infrastructure.Endpoints.Legal;

// Carries its own logic (the source IP for the audit record), so it has its own test:
// RecordAcceptanceEndpointTests.
public class RecordAcceptanceEndpoint(RecordAcceptanceCommandHandler recordAcceptance) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("v1/legal/acceptance");
        Description(b => b.ClearDefaultProduces(200).Produces(204));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sourceIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        await recordAcceptance.HandleAsync(new RecordAcceptanceCommand(sourceIp), ct);
        await SendNoContentAsync(ct);
    }
}
