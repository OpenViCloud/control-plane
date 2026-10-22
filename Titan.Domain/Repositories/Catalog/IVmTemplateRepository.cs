using Titan.Domain.Aggregates.Catalog.VmTemplate;

namespace Titan.Domain.Repositories.Catalog;

// Every read loads the zone rows: a template's state lives in them (https://github.com/OpenViCloud/titan-ops/blob/main/docs/features/catalog/design.md#template-per-zone).
public interface IVmTemplateRepository
{
    Task<VmTemplate?> FindByIdAsync(Guid id, CancellationToken ct = default);
    /// What customers in a zone may pick: templates that zone's provider was last observed to hold.
    Task<List<VmTemplate>> ListSyncedInZoneAsync(string zoneId, CancellationToken ct = default);
    /// Templates synced in at least one zone — what a VM anywhere can have been created from.
    Task<List<VmTemplate>> ListSyncedInAnyZoneAsync(CancellationToken ct = default);
    /// Templates with a provider id in the zone — what that zone's template reconciler can observe.
    Task<List<VmTemplate>> ListReconcilableInZoneAsync(string zoneId, CancellationToken ct = default);
    Task<List<VmTemplate>> ListAllAsync(CancellationToken ct = default);
    void Add(VmTemplate template);
    Task SaveAsync(CancellationToken ct = default);
}
