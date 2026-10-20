using FastEndpoints;
using Titan.Application.Queries.Billing.Invoice;
using Titan.Application.Queries.Billing.Invoice.Contracts;

namespace Titan.Infrastructure.Endpoints.Billing;

public class ListTenantInvoicesEndpoint(ListTenantInvoicesQueryHandler handler)
    : EndpointWithoutRequest<IReadOnlyList<InvoiceQueryResult>>
{
    public override void Configure()
    {
        Get("v1/tenant/billing/invoices");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(await handler.HandleAsync(new ListTenantInvoicesQuery(), ct), ct);
    }
}
