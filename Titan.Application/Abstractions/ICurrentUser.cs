namespace Titan.Application.Abstractions;

public interface ICurrentUser
{
    string KeycloakUserId { get; }
    Guid? TenantId { get; }   // null for a platform admin (belongs to no tenant)
    bool IsAdmin { get; }
}
