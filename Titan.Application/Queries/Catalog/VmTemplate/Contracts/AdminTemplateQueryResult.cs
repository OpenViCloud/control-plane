using VmTemplateAggregate = Titan.Domain.Aggregates.Catalog.VmTemplate.VmTemplate;

namespace Titan.Application.Queries.Catalog.VmTemplate.Contracts;

// Admin-facing — DELIBERATELY exposes the internal sync state of every zone that TemplateQueryResult hides.
public record AdminTemplateQueryResult(
    Guid Id,
    string Name,
    string? Description,
    string Os,           // = OsImageName
    string VmOfferingId, // https://github.com/OpenViCloud/titan/blob/main/docs/features/catalog/design.md#template-borrows-sizing — the tier this template borrows its base sizing from
    int? DiskGb,         // = RootDiskSizeGb
    bool Listed,         // https://github.com/OpenViCloud/titan/blob/main/docs/features/catalog/design.md#template-listed — false ⇒ customers neither see nor pick it
    List<AdminTemplateZoneQueryResult> Zones) // ordered by zone id
{
    public static AdminTemplateQueryResult From(VmTemplateAggregate t) =>
        new(t.Id, t.Name, t.Description, t.OsImageName, t.VmOfferingId, t.RootDiskSizeGb, t.IsListed,
            t.Zones.OrderBy(z => z.ZoneId, StringComparer.Ordinal).Select(AdminTemplateZoneQueryResult.From).ToList());
}
