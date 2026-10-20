namespace Titan.Application.Queries.Platform.ApiToken.Contracts;

/// Never the secret nor its hash: the prefix is how the customer tells tokens apart. `ExpiresAt` null: never expires.
public record ApiTokenQueryResult(Guid Id, string Name, string Prefix, DateTimeOffset CreatedAt, DateTimeOffset? ExpiresAt);
