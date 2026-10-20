using FastEndpoints;
using Titan.Application.Queries.Compute.SshKey;
using Titan.Application.Queries.Compute.SshKey.Contracts;

namespace Titan.Infrastructure.Endpoints.SshKey;

public class ListSshKeysEndpoint(ListSshKeysQueryHandler handler) : EndpointWithoutRequest<List<SshKeyQueryResult>>
{
    public override void Configure() => Get("v1/tenant/ssh-keys");

    public override async Task HandleAsync(CancellationToken ct) =>
        await SendOkAsync(await handler.HandleAsync(new ListSshKeysQuery(), ct), ct);
}
