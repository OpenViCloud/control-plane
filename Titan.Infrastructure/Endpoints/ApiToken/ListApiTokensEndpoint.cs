using FastEndpoints;
using Titan.Application.Queries.Platform.ApiToken;
using Titan.Application.Queries.Platform.ApiToken.Contracts;

namespace Titan.Infrastructure.Endpoints.ApiToken;

public class ListApiTokensEndpoint(ListApiTokensQueryHandler handler) : EndpointWithoutRequest<List<ApiTokenQueryResult>>
{
    public override void Configure() => Get("v1/tenant/api-tokens");

    public override async Task HandleAsync(CancellationToken ct) =>
        await SendOkAsync(await handler.HandleAsync(new ListApiTokensQuery(), ct), ct);
}
