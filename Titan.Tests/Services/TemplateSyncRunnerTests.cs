using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Titan.Application.Abstractions;
using Titan.Application.Options;
using Titan.Application.Services.Catalog.VmTemplate;
using Titan.Application.Commands.Catalog.VmTemplate;
using Titan.Domain.Aggregates.Catalog.VmTemplate;
using Titan.Domain.Repositories.Catalog;

namespace Titan.Tests.Services;

// Counts per (template, zone) over the two configured zones (docs/features/catalog/design.md#template-per-zone).
public class TemplateSyncRunnerTests
{
    private const string Mock = "mock", Eude = "eude01";

    private static readonly ZoneOptions Zones = new()
    {
        Zones = [new ZoneConfig { Id = Mock, Name = "Mock" }, new ZoneConfig { Id = Eude, Name = "EU DE 01" }],
    };

    private readonly Mock<IVmTemplateRepository> _repo = new();

    // The runner injects SyncTemplatesCommandHandler (concrete, no interface — docs/architecture/code-structure.md#no-mediatr), so
    // the enqueue step runs for real. It reads templates through its OWN repository mock: sharing
    // _repo would let the enqueue consume the first value of a poll sequence set up below, and the
    // tests here are about polling. The enqueue itself is pinned in TemplateServiceTests.
    private readonly Mock<IVmTemplateRepository> _syncRepo = new();
    private readonly Mock<IProviderCommandQueue> _commands = new();

    public TemplateSyncRunnerTests()
    {
        _syncRepo.Setup(r => r.ListAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _commands.SetReturnsDefault(Task.CompletedTask);
    }

    private TemplateSyncRunner Runner() =>
        new(new SyncTemplatesCommandHandler(_syncRepo.Object, Zones, _commands.Object,
                NullLogger<SyncTemplatesCommandHandler>.Instance),
            _repo.Object, Zones, NullLogger<TemplateSyncRunner>.Instance);

    // A runner that skipped the enqueue and only polled would otherwise stay green.
    [Fact]
    public async Task Run_EnqueuesEveryTemplate_BeforePolling()
    {
        var t = VmTemplate.Create("a", null, "img", 10, "s");
        _syncRepo.Setup(r => r.ListAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([t]);
        _repo.Setup(r => r.ListAllAsync(default)).ReturnsAsync([Template("a", "Exists", "Exists")]);

        await Runner().RunAsync(1, TimeSpan.Zero, CountingDelay(() => { }));

        foreach (string zoneId in new[] { Mock, Eude })
        {
            _commands.Verify(w => w.EnqueueTemplateSyncAsync(t.Id, zoneId, "a", null, "img", null, 10,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Fact]
    public async Task AllSyncedOnFirstPoll_ReturnsAllSynced_WithoutWaiting()
    {
        _repo.Setup(r => r.ListAllAsync(default))
            .ReturnsAsync([Template("a", "Exists", "Exists"), Template("b", "Exists", "Exists")]);
        var delays = 0;

        var res = await Runner().RunAsync(5, TimeSpan.Zero, CountingDelay(() => delays++));

        Assert.Equal(TemplateSyncOutcome.AllSynced, res.Outcome);
        Assert.Equal((4, 4), (res.Total, res.Synced)); // 2 templates × 2 zones
        Assert.Equal(0, delays); // already done — never sleeps
    }

    [Fact]
    public async Task PendingThenSynced_WaitsOnceThenSucceeds()
    {
        _repo.SetupSequence(r => r.ListAllAsync(default))
            .ReturnsAsync([Template("a", "Exists", "Waiting")])
            .ReturnsAsync([Template("a", "Exists", "Exists")]);
        var delays = 0;

        var res = await Runner().RunAsync(5, TimeSpan.Zero, CountingDelay(() => delays++));

        Assert.Equal(TemplateSyncOutcome.AllSynced, res.Outcome);
        Assert.Equal(1, delays);
    }

    [Fact]
    public async Task AlwaysPending_TimesOutAfterMaxAttempts()
    {
        _repo.Setup(r => r.ListAllAsync(default)).ReturnsAsync([Template("a", "Exists", "Waiting")]);
        var delays = 0;

        var res = await Runner().RunAsync(3, TimeSpan.Zero, CountingDelay(() => delays++));

        Assert.Equal(TemplateSyncOutcome.TimedOut, res.Outcome);
        Assert.Equal(1, res.Pending);
        Assert.Equal(2, delays); // polls 3 times, waits between the first two
    }

    [Fact] // a configured zone the template has no row in has not been synced there: it is not "done"
    public async Task AZoneWithNoRowYet_CountsPending_NotSynced()
    {
        _repo.Setup(r => r.ListAllAsync(default)).ReturnsAsync([Template("a", "Exists", "none")]);

        var res = await Runner().RunAsync(1, TimeSpan.Zero, CountingDelay(() => { }));

        Assert.Equal(TemplateSyncOutcome.TimedOut, res.Outcome);
        Assert.Equal((2, 1, 1), (res.Total, res.Synced, res.Pending));
    }

    [Fact]
    public async Task AnyFailed_ShortCircuits_WithoutWaiting()
    {
        // A Failed template won't self-heal within this run — surface it immediately
        // instead of burning the whole timeout.
        _repo.Setup(r => r.ListAllAsync(default))
            .ReturnsAsync([Template("a", "Exists", "Exists"), Template("b", "Failed", "Waiting")]);
        var delays = 0;

        var res = await Runner().RunAsync(5, TimeSpan.Zero, CountingDelay(() => delays++));

        Assert.Equal(TemplateSyncOutcome.HasFailures, res.Outcome);
        Assert.Equal(1, res.Failed);
        Assert.Equal(0, delays);
    }

    [Fact] // a zone asked again after a give-up is waiting, not failed: the reason stays until the worker answers
    public async Task AZoneRetryingAfterAGiveUp_CountsPending_NotFailed()
    {
        _repo.Setup(r => r.ListAllAsync(default)).ReturnsAsync([Template("a", "Exists", "Retrying")]);

        var res = await Runner().RunAsync(1, TimeSpan.Zero, CountingDelay(() => { }));

        Assert.Equal((2, 1, 0, 1), (res.Total, res.Synced, res.Failed, res.Pending));
        Assert.Equal(TemplateSyncOutcome.TimedOut, res.Outcome);
    }

    [Fact] // D-5: the verdict covers the whole catalog — one failed zone fails the run though the other synced
    public async Task OneZoneFailed_IsHasFailures_EvenWhenTheOtherZoneSynced()
    {
        _repo.Setup(r => r.ListAllAsync(default)).ReturnsAsync([Template("a", "Exists", "Failed")]);

        var res = await Runner().RunAsync(5, TimeSpan.Zero, CountingDelay(() => { }));

        Assert.Equal(TemplateSyncOutcome.HasFailures, res.Outcome);
        Assert.Equal((2, 1, 1), (res.Total, res.Synced, res.Failed));
    }

    // A run with no zone configured syncs nothing and has nothing left pending: counted as done it would report a
    // catalog no customer can use as AllSynced. The deploy Job once ran without the zone env exactly like that.
    [Fact]
    public async Task NoZoneConfigured_IsNoZones_NotAllSynced()
    {
        _repo.Setup(r => r.ListAllAsync(default)).ReturnsAsync([Template("a", "none", "none")]);
        var runner = new TemplateSyncRunner(
            new SyncTemplatesCommandHandler(_syncRepo.Object, new ZoneOptions(), _commands.Object,
                NullLogger<SyncTemplatesCommandHandler>.Instance),
            _repo.Object, new ZoneOptions(), NullLogger<TemplateSyncRunner>.Instance);

        var res = await runner.RunAsync(5, TimeSpan.Zero, CountingDelay(() => { }));

        Assert.Equal(TemplateSyncOutcome.NoZones, res.Outcome);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static Func<TimeSpan, CancellationToken, Task> CountingDelay(Action onDelay) =>
        (_, _) => { onDelay(); return Task.CompletedTask; };

    // A template's sync in mock and in eude01: "none" (never asked) | "Waiting" | "Exists" | "Failed" |
    // "Retrying" (failed, then asked again).
    private static VmTemplate Template(string name, string inMock, string inEude)
    {
        var t = VmTemplate.Create(name, null, "os-image", 10, "m");
        Put(t, Mock, inMock);
        Put(t, Eude, inEude);
        return t;
    }

    private static void Put(VmTemplate t, string zoneId, string state)
    {
        if (state == "none")
        {
            return;
        }
        t.RequestSyncIn(zoneId);
        if (state == "Exists")
        {
            t.MarkCreatedIn(zoneId, $"ext-{t.Name}-{zoneId}");
        }
        if (state == "Failed")
        {
            t.MarkRequestFailedIn(zoneId, "provider unreachable", null);
        }
        if (state == "Retrying")
        {
            t.MarkRequestFailedIn(zoneId, "provider unreachable", null);
            t.RequestSyncIn(zoneId);
        }
    }
}
