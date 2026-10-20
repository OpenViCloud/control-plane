using FastEndpoints;
using Titan.Application.Queries.Catalog.VmOffering;
using Titan.Application.Queries.Catalog.VmOffering.Contracts;

namespace Titan.Infrastructure.Endpoints.Catalog;

// Titan-owned, never synced to the provider (docs/features/catalog/design.md#vm-offering-sizing).
public class ListVmOfferingsEndpoint(ListVmOfferingsQueryHandler handler)
    : EndpointWithoutRequest<List<VmOfferingQueryResult>>
{
    public override void Configure()
    {
        Get("v1/vm-offerings");
        // Anonymous: the public site shows prices before sign-up (docs/features/public-site/design.md#live-pricing).
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(await handler.HandleAsync(new ListVmOfferingsQuery(), ct), cancellation: ct);
    }
}
