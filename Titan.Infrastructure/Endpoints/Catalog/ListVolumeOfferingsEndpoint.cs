using FastEndpoints;
using Titan.Application.Queries.Catalog.VolumeOffering;
using Titan.Application.Queries.Catalog.VolumeOffering.Contracts;

namespace Titan.Infrastructure.Endpoints.Catalog;

// Served from Titan's DB, not live from the provider: storage placement is a worker concern
// (https://github.com/OpenViCloud/titan-ops/blob/main/docs/features/catalog/design.md#volume-offering-catalog).
public class ListVolumeOfferingsEndpoint(ListVolumeOfferingsQueryHandler handler)
    : EndpointWithoutRequest<List<VolumeOfferingQueryResult>>
{
    public override void Configure()
    {
        Get("v1/volume-offerings");
        // Anonymous: the public site shows prices before sign-up (https://github.com/OpenViCloud/titan-ops/blob/main/docs/features/public-site/design.md#live-pricing).
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(await handler.HandleAsync(new ListVolumeOfferingsQuery(), ct), cancellation: ct);
    }
}
