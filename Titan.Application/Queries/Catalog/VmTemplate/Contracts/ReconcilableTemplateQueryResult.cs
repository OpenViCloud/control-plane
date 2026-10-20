using VmTemplateAggregate = Titan.Domain.Aggregates.Catalog.VmTemplate.VmTemplate;
using Titan.Domain.Aggregates.Catalog.VmTemplate;

namespace Titan.Application.Queries.Catalog.VmTemplate.Contracts;

// What the reconciler of one zone needs to match an image alias on that zone's cluster to a Titan template and to
// report only what changed. Internal only — never on a customer path.
public record ReconcilableTemplateQueryResult(Guid Id, string ExternalTemplateId, string CurrentExistence)
{
    // The repository lists only templates with a provider id in the zone, so a missing one here is a broken
    // query, not a row to skip quietly.
    public static ReconcilableTemplateQueryResult From(VmTemplateAggregate t, string zoneId)
    {
        VmTemplateZone? zone = t.ZoneOrNull(zoneId);
        string? externalTemplateId = null;
        if (zone != null)
        {
            externalTemplateId = zone.ExternalTemplateId;
        }
        if (string.IsNullOrEmpty(externalTemplateId))
        {
            throw new InvalidOperationException($"Template {t.Id} has no provider id in zone {zoneId} and cannot be reconciled.");
        }
        return new(t.Id, externalTemplateId, zone!.CurrentExistence.ToString());
    }
}
