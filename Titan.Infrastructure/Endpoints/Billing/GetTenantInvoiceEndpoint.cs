using FastEndpoints;
using Titan.Application.Queries.Billing.Invoice;
using Titan.Application.Queries.Billing.Invoice.Contracts;

namespace Titan.Infrastructure.Endpoints.Billing;

public class GetTenantInvoiceEndpoint(GetTenantInvoiceQueryHandler handler)
    : EndpointWithoutRequest<InvoiceDetailQueryResult>
{
    public override void Configure()
    {
        Get("v1/tenant/billing/invoices/{invoiceId:guid}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var invoiceId = Route<Guid>("invoiceId");
        await SendOkAsync(await handler.HandleAsync(new GetTenantInvoiceQuery(invoiceId), ct), ct);
    }
}
