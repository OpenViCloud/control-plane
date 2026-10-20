using Titan.Application.Queries.Catalog.VmTemplate.Contracts;
using Titan.Domain.Repositories.Catalog;

namespace Titan.Application.Queries.Catalog.VmTemplate;

// Read side of the template reconciler of one zone (docs/architecture/desired-and-current.md#desired-and-current).
public class ListReconcilableTemplatesQueryHandler(IVmTemplateRepository templates)
{
    public async Task<List<ReconcilableTemplateQueryResult>> HandleAsync(ListReconcilableTemplatesQuery query, CancellationToken ct = default)
    {
        var rows = await templates.ListReconcilableInZoneAsync(query.ZoneId, ct);
        return rows.Select(t => ReconcilableTemplateQueryResult.From(t, query.ZoneId)).ToList();
    }
}
