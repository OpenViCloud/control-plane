namespace Titan.Application.Commands.Platform.Tenant.Contracts;

// The API keeps decimal; the handler casts to long VND at the boundary (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/money-and-wallet.md#money-is-long-vnd).
public record UpdateTenantDebtCeilingCommand(Guid TenantId, decimal DebtCeiling);
