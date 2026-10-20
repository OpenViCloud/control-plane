using FastEndpoints;
using Titan.Application.Queries.Billing.Wallet;
using Titan.Application.Queries.Billing.Wallet.Contracts;

namespace Titan.Infrastructure.Endpoints.Wallet;

// https://github.com/OpenViCloud/titan/blob/main/docs/architecture/http-contract.md#tenant-scoping
public class GetWalletEndpoint(GetWalletQueryHandler handler) : EndpointWithoutRequest<WalletQueryResult>
{
    public override void Configure() => Get("v1/tenant/wallet");

    public override async Task HandleAsync(CancellationToken ct) =>
        await SendOkAsync(await handler.HandleAsync(new GetWalletQuery(), ct), ct);
}
