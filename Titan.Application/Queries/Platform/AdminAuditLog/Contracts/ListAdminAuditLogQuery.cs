namespace Titan.Application.Queries.Platform.AdminAuditLog.Contracts;

// Limit <= 0 means "not given" and falls back to the default; the handler clamps it.
public record ListAdminAuditLogQuery(int Limit);
