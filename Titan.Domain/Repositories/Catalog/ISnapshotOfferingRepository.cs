using Titan.Domain.Aggregates.Catalog.SnapshotOffering;

namespace Titan.Domain.Repositories.Catalog;

public interface ISnapshotOfferingRepository
{
    Task<SnapshotOffering?> FindByIdAsync(string id, CancellationToken ct = default);
    Task<List<SnapshotOffering>> ListAllAsync(CancellationToken ct = default);
}
