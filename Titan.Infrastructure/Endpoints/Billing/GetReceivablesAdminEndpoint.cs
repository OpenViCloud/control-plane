using FastEndpoints;
using Titan.Application.Queries.Billing.Invoice;
using Titan.Application.Queries.Billing.Invoice.Contracts;

namespace Titan.Infrastructure.Endpoints.Billing;

// https://github.com/OpenViCloud/titan/blob/main/docs/architecture/money-and-wallet.md#postpaid-no-brake. Overdue is derived at read time
// (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/money-and-wallet.md#invoice-frozen-snapshot), so this is the only aggregate view of it.
public class GetReceivablesAdminEndpoint(GetReceivablesQueryHandler handler)
    : EndpointWithoutRequest<ReceivablesQueryResult>
{
    public override void Configure()
    {
        Get("v1/admin/billing/receivables");
        Roles("platform-admin");
    }

    public override async Task HandleAsync(CancellationToken ct) =>
        await SendOkAsync(await handler.HandleAsync(new GetReceivablesQuery(), ct), ct);
}
