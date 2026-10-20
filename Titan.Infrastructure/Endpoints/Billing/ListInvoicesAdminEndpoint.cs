using FastEndpoints;
using Titan.Application.Queries.Billing.Invoice;
using Titan.Application.Queries.Billing.Invoice.Contracts;

namespace Titan.Infrastructure.Endpoints.Billing;

public class ListInvoicesAdminEndpoint(ListInvoicesQueryHandler handler)
    : EndpointWithoutRequest<IReadOnlyList<InvoiceQueryResult>>
{
    public override void Configure()
    {
        Get("v1/admin/invoices");
        Roles("platform-admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var status = Query<string>("status", isRequired: false);
        await SendOkAsync(await handler.HandleAsync(new ListInvoicesQuery(status), ct), ct);
    }
}
