namespace Titan.Application.Commands.Billing.TopUp.Contracts;

// The body of POST /v1/admin/topups/grant. Amount is whole VND.
public record GrantTopUpCommand(Guid TenantId, long Amount, string? Reason);
