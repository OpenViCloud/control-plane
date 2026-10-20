using Titan.Application.Abstractions;

namespace Titan.Application.Services.Platform;


public interface IIdempotencyService
{
    Task<IdempotencyDecision> BeginAsync(Guid tenantId, string key, string requestHash, CancellationToken ct = default);
    Task CompleteAsync(Guid tenantId, string key, int statusCode, string responseBody, CancellationToken ct = default);
    Task AbandonAsync(Guid tenantId, string key, CancellationToken ct = default);
}

public class IdempotencyService(IIdempotencyStore store) : IIdempotencyService
{
    public async Task<IdempotencyDecision> BeginAsync(Guid tenantId, string key, string requestHash, CancellationToken ct = default)
    {
        var existing = await store.FindAsync(tenantId, key, ct);
        if (existing is not null)
            return Decide(existing, requestHash);

        if (await store.TryBeginAsync(tenantId, key, requestHash, ct))
            return new IdempotencyDecision.Proceed();

        // A concurrent request inserted between FindAsync and TryBeginAsync (unique violation).
        var raced = await store.FindAsync(tenantId, key, ct);
        return raced is null
            ? new IdempotencyDecision.InProgress()
            : Decide(raced, requestHash);
    }

    private static IdempotencyDecision Decide(IdempotencyEntry e, string requestHash)
    {
        if (e.RequestHash != requestHash)
            return new IdempotencyDecision.KeyReused();

        if (e.Status == IdempotencyStatus.Completed && e.ResponseStatusCode is int statusCode)
            return new IdempotencyDecision.Replay(statusCode, e.ResponseBody ?? string.Empty);

        return new IdempotencyDecision.InProgress();
    }

    public Task CompleteAsync(Guid tenantId, string key, int statusCode, string responseBody, CancellationToken ct = default)
        => store.CompleteAsync(tenantId, key, statusCode, responseBody, ct);

    public Task AbandonAsync(Guid tenantId, string key, CancellationToken ct = default)
        => store.RemoveAsync(tenantId, key, ct);
}
