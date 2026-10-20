namespace Titan.Application.Commands.Billing.Wallet.Contracts;

// Not tenant-scoped — the provision callback passes the tenant being activated.
public record OpenWalletCommand(Guid TenantId);
