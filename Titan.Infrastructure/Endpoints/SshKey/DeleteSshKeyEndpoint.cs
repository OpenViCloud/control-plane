using Microsoft.AspNetCore.Http;
using FastEndpoints;
using Titan.Application.Commands.Compute.SshKey;
using Titan.Application.Commands.Compute.SshKey.Contracts;

namespace Titan.Infrastructure.Endpoints.SshKey;

public class DeleteSshKeyEndpoint(DeleteSshKeyCommandHandler handler) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("v1/tenant/ssh-keys/{sshKeyId:guid}");
        Description(b => b.ClearDefaultProduces(200).Produces(204));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sshKeyId = Route<Guid>("sshKeyId");
        await handler.HandleAsync(new DeleteSshKeyCommand(sshKeyId), ct);
        await SendNoContentAsync(ct);
    }
}
