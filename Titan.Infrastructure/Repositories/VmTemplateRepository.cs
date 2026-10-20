using Microsoft.EntityFrameworkCore;
using Titan.Domain.Aggregates.Catalog.VmTemplate;
using Titan.Domain.Repositories.Catalog;
using Titan.Domain.Values;
using Titan.Infrastructure.Persistence;

namespace Titan.Infrastructure.Repositories;

public class VmTemplateRepository(TitanDbContext db) : IVmTemplateRepository
{
    public async Task<VmTemplate?> FindByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.VmTemplates.Include(t => t.Zones).FirstOrDefaultAsync(t => t.Id == id, ct);

    // Unordered: ListTemplatesQueryHandler owns the customer-facing name ordering.
    public async Task<List<VmTemplate>> ListSyncedInZoneAsync(string zoneId, CancellationToken ct = default) =>
        await db.VmTemplates
            .Include(t => t.Zones)
            .Where(t => t.Zones.Any(z => z.ZoneId == zoneId && z.CurrentExistence == CurrentExistence.Exists))
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<List<VmTemplate>> ListSyncedInAnyZoneAsync(CancellationToken ct = default) =>
        await db.VmTemplates
            .Include(t => t.Zones)
            .Where(t => t.Zones.Any(z => z.CurrentExistence == CurrentExistence.Exists))
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<List<VmTemplate>> ListReconcilableInZoneAsync(string zoneId, CancellationToken ct = default) =>
        await db.VmTemplates
            .Include(t => t.Zones)
            .Where(t => t.Zones.Any(z => z.ZoneId == zoneId && z.ExternalTemplateId != null))
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<List<VmTemplate>> ListAllAsync(CancellationToken ct = default) =>
        await db.VmTemplates
            .Include(t => t.Zones)
            .OrderBy(t => t.Name)
            .AsNoTracking()
            .ToListAsync(ct);

    public void Add(VmTemplate template) => db.VmTemplates.Add(template);

    public async Task SaveAsync(CancellationToken ct = default) => await db.SaveChangesAsync(ct);
}
