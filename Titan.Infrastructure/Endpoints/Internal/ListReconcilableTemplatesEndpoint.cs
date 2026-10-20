using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Titan.Application.Queries.Catalog.VmTemplate;
using Titan.Application.Queries.Catalog.VmTemplate.Contracts;
using Titan.Infrastructure.Hosting;

namespace Titan.Infrastructure.Endpoints.Internal;

// One template reconciler per zone (docs/features/catalog/design.md#template-per-zone).
public class ListReconcilableTemplatesEndpoint(
    ListReconcilableTemplatesQueryHandler handler,
    ReconcilerHeartbeat heartbeat,
    IConfiguration config)
    : EndpointWithoutRequest<List<ReconcilableTemplateQueryResult>>
{
    public override void Configure()
    {
        Get("internal/reconcile/templates");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var token = HttpContext.Request.Headers["X-Internal-Token"].FirstOrDefault();
        if (!InternalTokenAuth.Matches(token, config["Internal:Token"]))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        string? zoneId = Query<string>("zoneId", isRequired: false);
        if (string.IsNullOrEmpty(zoneId))
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        List<ReconcilableTemplateQueryResult> rows = await handler.HandleAsync(new ListReconcilableTemplatesQuery(zoneId), ct);
        heartbeat.RecordPoll(ReconcilerHeartbeat.KeyFor(ReconcilerHeartbeat.TemplateKind, zoneId), DateTimeOffset.UtcNow);
        await SendAsync(rows, cancellation: ct);
    }
}
