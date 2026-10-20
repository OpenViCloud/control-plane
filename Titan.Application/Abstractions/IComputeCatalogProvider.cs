namespace Titan.Application.Abstractions;

public interface IComputeCatalogProvider
{
    Task<List<ClusterInfo>> ListClustersAsync(string zoneId, CancellationToken ct = default);
    Task<List<OsImageInfo>> ListOsImagesAsync(CancellationToken ct = default);
}

public record ZoneInfo(string Id, string Name);
public record ClusterInfo(string Id, string Name);
public record OsImageInfo(string Id, string Name, int SizeGb);
