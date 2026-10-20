using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Titan.Infrastructure.Auth;

// Keycloak nests realm roles in `realm_access.roles`, but RequireRole/IsInRole only read flat role claims:
// without this every Roles("platform-admin") endpoint 403s a valid admin token. The HasClaim guard keeps it
// idempotent, since ASP.NET may invoke IClaimsTransformation more than once per request.
public sealed class KeycloakRealmRolesClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            return Task.FromResult(principal);

        var realmAccess = principal.FindFirst("realm_access")?.Value;
        if (string.IsNullOrEmpty(realmAccess))
            return Task.FromResult(principal);

        try
        {
            using var doc = JsonDocument.Parse(realmAccess);
            if (doc.RootElement.TryGetProperty("roles", out var roles) &&
                roles.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in roles.EnumerateArray())
                {
                    var name = role.GetString();
                    if (!string.IsNullOrEmpty(name) && !identity.HasClaim(identity.RoleClaimType, name))
                        identity.AddClaim(new Claim(identity.RoleClaimType, name));
                }
            }
        }
        catch (JsonException)
        {
            // Malformed realm_access → no roles projected; authentication itself already succeeded.
        }

        return Task.FromResult(principal);
    }
}
