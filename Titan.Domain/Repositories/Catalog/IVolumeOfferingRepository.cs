using Titan.Domain.Aggregates.Catalog.VolumeOffering;

namespace Titan.Domain.Repositories.Catalog;

public interface IVolumeOfferingRepository
{
    Task<VolumeOffering?> FindByIdAsync(string id, CancellationToken ct = default);
    Task<List<VolumeOffering>> ListAllAsync(CancellationToken ct = default);
}
