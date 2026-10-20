using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Titan.Application.Abstractions;

namespace Titan.Infrastructure.Auth;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal User => httpContextAccessor.HttpContext!.User;

    public string KeycloakUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? throw new InvalidOperationException("User identity not found in token.");

    public Guid? TenantId =>
        httpContextAccessor.HttpContext?.Items["TenantId"] as Guid?;

    public bool IsAdmin => User.IsInRole("platform-admin");
}
