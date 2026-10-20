namespace Titan.Application.Commands.Platform.Tenant.Contracts;

// Status is the raw string from the admin request; the handler parses it (INVALID_STATUS on an unknown value).
public record UpdateTenantStatusCommand(Guid TenantId, string Status);
