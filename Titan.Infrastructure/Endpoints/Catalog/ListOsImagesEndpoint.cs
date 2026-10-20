using FastEndpoints;
using Titan.Application.Abstractions;
using Titan.Application.Queries.Catalog.OsImage;
using Titan.Application.Queries.Catalog.OsImage.Contracts;

namespace Titan.Infrastructure.Endpoints.Catalog;

public class ListOsImagesEndpoint(ListOsImagesQueryHandler handler) : EndpointWithoutRequest<List<OsImageInfo>>
{
    public override void Configure()
    {
        Get("v1/os-images");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(await handler.HandleAsync(new ListOsImagesQuery(), ct), cancellation: ct);
    }
}
