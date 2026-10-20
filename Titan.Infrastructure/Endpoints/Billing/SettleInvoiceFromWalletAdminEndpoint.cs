using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Titan.Application.Commands.Billing.Invoice;
using Titan.Application.Commands.Billing.Invoice.Contracts;

namespace Titan.Infrastructure.Endpoints.Billing;

/// A different route from MarkInvoicePaidEndpoint on purpose: that one records money that arrived outside
/// Titan, this one debits the wallet, and an audit log must tell them apart
/// (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/money-and-wallet.md#human-confirmed-debit). 202: the debit is queued, not done
/// (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/http-contract.md#async-actions-return-202).
public class SettleInvoiceFromWalletAdminEndpoint(SettleInvoiceFromWalletCommandHandler handler) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("v1/admin/invoices/{invoiceId:guid}/settle-from-wallet");
        Roles("platform-admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var invoiceId = Route<Guid>("invoiceId");
        await handler.HandleAsync(new SettleInvoiceFromWalletCommand(invoiceId), ct);
        await SendResultAsync(TypedResults.StatusCode(StatusCodes.Status202Accepted));
    }
}
