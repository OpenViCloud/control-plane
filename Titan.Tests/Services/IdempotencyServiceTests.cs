using Moq;
using Titan.Application.Abstractions;
using Titan.Application.Services.Platform;

namespace Titan.Tests.Services;

public class IdempotencyServiceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private const string Key = "key-123";
    private const string HashA = "AAAA";
    private const string HashB = "BBBB";

    // ── first time → Proceed + record reserved ─────────────────
    [Fact]
    public async Task FirstRequest_ReturnsProceed_AndReservesRecord()
    {
        var store = new InMemoryStore();
        var svc = new IdempotencyService(store);

        var decision = await svc.BeginAsync(Tenant, Key, HashA);

        Assert.IsType<IdempotencyDecision.Proceed>(decision);
        var entry = await store.FindAsync(Tenant, Key);
        Assert.NotNull(entry);
        Assert.Equal(IdempotencyStatus.InProgress, entry!.Status);
        Assert.Equal(HashA, entry.RequestHash);
    }

    // ── same key+hash, completed → Replay stored response ──────
    [Fact]
    public async Task SameKeySameHash_Completed_ReturnsReplay()
    {
        var store = new InMemoryStore();
        var svc = new IdempotencyService(store);
        await svc.BeginAsync(Tenant, Key, HashA);
        await svc.CompleteAsync(Tenant, Key, 202, "{\"vmId\":\"abc\"}");

        var decision = await svc.BeginAsync(Tenant, Key, HashA);

        var replay = Assert.IsType<IdempotencyDecision.Replay>(decision);
        Assert.Equal(202, replay.StatusCode);
        Assert.Equal("{\"vmId\":\"abc\"}", replay.Body);
    }

    // ── same key, different body → KeyReused ──────────────────
    [Fact]
    public async Task SameKeyDifferentHash_ReturnsKeyReused()
    {
        var store = new InMemoryStore();
        var svc = new IdempotencyService(store);
        await svc.BeginAsync(Tenant, Key, HashA);
        await svc.CompleteAsync(Tenant, Key, 202, "body");

        var decision = await svc.BeginAsync(Tenant, Key, HashB);

        Assert.IsType<IdempotencyDecision.KeyReused>(decision);
    }

    // ── same key, still in-progress → InProgress ──────────────
    [Fact]
    public async Task SameKey_StillInProgress_ReturnsInProgress()
    {
        var store = new InMemoryStore();
        var svc = new IdempotencyService(store);
        await svc.BeginAsync(Tenant, Key, HashA); // begun but not completed

        var decision = await svc.BeginAsync(Tenant, Key, HashA);

        Assert.IsType<IdempotencyDecision.InProgress>(decision);
    }

    // ── same key across tenants is independent ────────────────
    [Fact]
    public async Task SameKeyDifferentTenant_ReturnsProceed()
    {
        var store = new InMemoryStore();
        var svc = new IdempotencyService(store);
        var tenant2 = Guid.NewGuid();
        await svc.BeginAsync(Tenant, Key, HashA);

        var decision = await svc.BeginAsync(tenant2, Key, HashA);

        Assert.IsType<IdempotencyDecision.Proceed>(decision);
    }

    // ── complete then replay ──────────────────────────────────
    [Fact]
    public async Task AfterComplete_SubsequentBegin_Replays()
    {
        var store = new InMemoryStore();
        var svc = new IdempotencyService(store);
        await svc.BeginAsync(Tenant, Key, HashA);
        await svc.CompleteAsync(Tenant, Key, 201, "created");

        var decision = await svc.BeginAsync(Tenant, Key, HashA);

        var replay = Assert.IsType<IdempotencyDecision.Replay>(decision);
        Assert.Equal(201, replay.StatusCode);
        Assert.Equal("created", replay.Body);
    }

    // ── abandon then retry re-attempts ────────────────────────
    [Fact]
    public async Task AfterAbandon_SubsequentBegin_ProceedsAgain()
    {
        var store = new InMemoryStore();
        var svc = new IdempotencyService(store);
        await svc.BeginAsync(Tenant, Key, HashA);
        await svc.AbandonAsync(Tenant, Key);

        var decision = await svc.BeginAsync(Tenant, Key, HashA);

        Assert.IsType<IdempotencyDecision.Proceed>(decision);
    }

    // ── race: TryBegin loses, re-find returns in-progress ────
    [Fact]
    public async Task Race_TryBeginFails_ReFindsInProgress_ReturnsInProgress()
    {
        var store = new Mock<IIdempotencyStore>();
        store.SetupSequence(s => s.FindAsync(Tenant, Key, It.IsAny<CancellationToken>()))
             .ReturnsAsync((IdempotencyEntry?)null)  // first look: not found
             .ReturnsAsync(new IdempotencyEntry(Tenant, Key, HashA, IdempotencyStatus.InProgress, null, null)); // after losing the race
        store.Setup(s => s.TryBeginAsync(Tenant, Key, HashA, It.IsAny<CancellationToken>()))
             .ReturnsAsync(false); // concurrent request won the insert
        var svc = new IdempotencyService(store.Object);

        var decision = await svc.BeginAsync(Tenant, Key, HashA);

        Assert.IsType<IdempotencyDecision.InProgress>(decision);
    }

    // ── In-memory store stub ──────────────────────────────────────────────
    private sealed class InMemoryStore : IIdempotencyStore
    {
        private readonly Dictionary<(Guid, string), IdempotencyEntry> _data = new();

        public Task<IdempotencyEntry?> FindAsync(Guid tenantId, string key, CancellationToken ct = default)
            => Task.FromResult(_data.TryGetValue((tenantId, key), out var e) ? e : null);

        public Task<bool> TryBeginAsync(Guid tenantId, string key, string requestHash, CancellationToken ct = default)
        {
            if (_data.ContainsKey((tenantId, key))) return Task.FromResult(false);
            _data[(tenantId, key)] = new IdempotencyEntry(tenantId, key, requestHash, IdempotencyStatus.InProgress, null, null);
            return Task.FromResult(true);
        }

        public Task CompleteAsync(Guid tenantId, string key, int statusCode, string responseBody, CancellationToken ct = default)
        {
            _data[(tenantId, key)] = _data[(tenantId, key)] with
            {
                Status = IdempotencyStatus.Completed,
                ResponseStatusCode = statusCode,
                ResponseBody = responseBody
            };
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid tenantId, string key, CancellationToken ct = default)
        {
            _data.Remove((tenantId, key));
            return Task.CompletedTask;
        }

        public Task<int> PurgeExpiredAsync(DateTimeOffset olderThan, CancellationToken ct = default)
            => Task.FromResult(0); // not exercised by IdempotencyService tests
    }
}
