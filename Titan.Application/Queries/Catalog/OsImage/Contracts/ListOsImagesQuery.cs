namespace Titan.Application.Queries.Catalog.OsImage.Contracts;

// OS images are read live from the compute provider — no Domain aggregate behind this folder.
// The result type is the provider port's OsImageInfo.
public record ListOsImagesQuery();
