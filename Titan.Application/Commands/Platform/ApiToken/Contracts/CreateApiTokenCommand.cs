namespace Titan.Application.Commands.Platform.ApiToken.Contracts;

/// `token` is the secret the portal made (ovc_ + 43 base64url characters); only its hash is kept.
/// `expiresInDays` is 30, 90 or 365, or null for a token that never expires.
public record CreateApiTokenCommand(string Name, string Token, int? ExpiresInDays);
