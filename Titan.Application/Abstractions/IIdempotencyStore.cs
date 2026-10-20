namespace Titan.Application.Abstractions;

public enum IdempotencyStatus { InProgress, Completed }

/// <summary>
/// Snapshot of a stored idempotency record, scoped by (TenantId, Key).
/// </summary>
public record IdempotencyEntry(
    Guid TenantId,
    string Key,
    string RequestHash,
    IdempotencyStatus Status,
    int? ResponseStatusCode,
    string? ResponseBody);

/// <summary>
/// Persistence for idempotency records. Implementations must enforce a unique
/// constraint on (TenantId, Key) so <see cref="TryBeginAsync"/> is race-safe.
/// </summary>
public interface IIdempotencyStore
{
    Task<IdempotencyEntry?> FindAsync(Guid tenantId, string key, CancellationToken ct = default);

    /// <summary>Insert an InProgress record. Returns false if one already exists (unique violation).</summary>
    Task<bool> TryBeginAsync(Guid tenantId, string key, string requestHash, CancellationToken ct = default);

    Task CompleteAsync(Guid tenantId, string key, int statusCode, string responseBody, CancellationToken ct = default);

    Task RemoveAsync(Guid tenantId, string key, CancellationToken ct = default);

    /// <summary>For TTL cleanup. Returns rows removed.</summary>
    Task<int> PurgeExpiredAsync(DateTimeOffset olderThan, CancellationToken ct = default);
}
