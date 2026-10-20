using Microsoft.EntityFrameworkCore;
using Titan.Domain.Aggregates.Catalog.VolumeOffering;
using Titan.Domain.Repositories.Catalog;
using Titan.Infrastructure.Persistence;

namespace Titan.Infrastructure.Repositories;

public class VolumeOfferingRepository(TitanDbContext db) : IVolumeOfferingRepository
{
    public async Task<VolumeOffering?> FindByIdAsync(string id, CancellationToken ct = default) =>
        await db.VolumeOfferings.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);

    // Unordered: ListVolumeOfferingsQueryHandler owns the customer-facing ordering.
    public async Task<List<VolumeOffering>> ListAllAsync(CancellationToken ct = default) =>
        await db.VolumeOfferings.AsNoTracking().ToListAsync(ct);
}
