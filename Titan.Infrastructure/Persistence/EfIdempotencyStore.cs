using Microsoft.EntityFrameworkCore;
using Titan.Application.Abstractions;

namespace Titan.Infrastructure.Persistence;

public class EfIdempotencyStore(TitanDbContext db) : IIdempotencyStore
{
    public async Task<IdempotencyEntry?> FindAsync(Guid tenantId, string key, CancellationToken ct = default)
    {
        var r = await db.IdempotencyRecords.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == key, ct);
        if (r is null) return null;

        return new IdempotencyEntry(
            r.TenantId, r.Key, r.RequestHash,
            r.Status == "Completed" ? IdempotencyStatus.Completed : IdempotencyStatus.InProgress,
            r.ResponseStatusCode, r.ResponseBody);
    }

    public async Task<bool> TryBeginAsync(Guid tenantId, string key, string requestHash, CancellationToken ct = default)
    {
        db.IdempotencyRecords.Add(new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Key = key,
            RequestHash = requestHash,
            Status = "InProgress",
            CreatedAt = DateTimeOffset.UtcNow
        });

        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException)
        {
            // Unique (tenant_id, key) violated — a concurrent request won the insert.
            db.ChangeTracker.Clear();
            return false;
        }
    }

    public Task CompleteAsync(Guid tenantId, string key, int statusCode, string responseBody, CancellationToken ct = default) =>
        db.IdempotencyRecords
            .Where(x => x.TenantId == tenantId && x.Key == key)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Status, "Completed")
                .SetProperty(x => x.ResponseStatusCode, (int?)statusCode)
                .SetProperty(x => x.ResponseBody, responseBody)
                .SetProperty(x => x.CompletedAt, (DateTimeOffset?)DateTimeOffset.UtcNow), ct);

    public Task RemoveAsync(Guid tenantId, string key, CancellationToken ct = default) =>
        db.IdempotencyRecords
            .Where(x => x.TenantId == tenantId && x.Key == key)
            .ExecuteDeleteAsync(ct);

    public Task<int> PurgeExpiredAsync(DateTimeOffset olderThan, CancellationToken ct = default) =>
        db.IdempotencyRecords
            .Where(x => x.CreatedAt < olderThan)
            .ExecuteDeleteAsync(ct);
}
