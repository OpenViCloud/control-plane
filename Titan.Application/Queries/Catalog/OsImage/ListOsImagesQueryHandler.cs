using Titan.Application.Abstractions;
using Titan.Application.Queries.Catalog.OsImage.Contracts;

namespace Titan.Application.Queries.Catalog.OsImage;

public class ListOsImagesQueryHandler(IComputeCatalogProvider provider)
{
    public Task<List<OsImageInfo>> HandleAsync(ListOsImagesQuery query, CancellationToken ct = default) =>
        provider.ListOsImagesAsync(ct);
}
