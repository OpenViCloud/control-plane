using Titan.Application.Services.Billing.Metering;

namespace Titan.Tests.Services;

// The liveness ledger the meter host writes and /healthz/meters reads. A failed tick must not erase
// the evidence of the last good one: a meter that succeeded an hour ago and has failed every tick
// since is exactly what the health check has to see, and eventually flag.
public class MeterHeartbeatTests
{
    private static readonly DateTimeOffset T0 = new(2026, 8, 31, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Register_declares_a_meter_with_no_tick_yet()
    {
        var hb = new MeterHeartbeat();
        hb.Register("Compute", T0);

        var state = hb.Snapshot()["Compute"];

        Assert.Equal(T0, state.RegisteredAt);
        Assert.Null(state.LastSuccessAt);
        Assert.Null(state.LastAttemptAt);
    }

    [Fact]
    public void Record_success_advances_LastSuccessAt()
    {
        var hb = new MeterHeartbeat();
        hb.Register("Compute", T0);

        hb.Record("Compute", success: true, T0.AddMinutes(10));

        var state = hb.Snapshot()["Compute"];
        Assert.Equal(T0.AddMinutes(10), state.LastSuccessAt);
        Assert.Equal(T0.AddMinutes(10), state.LastAttemptAt);
    }

    // A failed tick after a success keeps the last success time (so staleness is measured from the
    // last GOOD tick) but does advance the attempt time.
    [Fact]
    public void Record_failure_keeps_last_success_but_advances_attempt()
    {
        var hb = new MeterHeartbeat();
        hb.Register("Compute", T0);
        hb.Record("Compute", success: true, T0.AddMinutes(10));

        hb.Record("Compute", success: false, T0.AddMinutes(20));

        var state = hb.Snapshot()["Compute"];
        Assert.Equal(T0.AddMinutes(10), state.LastSuccessAt); // unchanged
        Assert.Equal(T0.AddMinutes(20), state.LastAttemptAt); // advanced
    }

    // A meter can Record before Register (or Register can arrive late); a late Register must not wipe
    // an already-recorded success back to never-ticked.
    [Fact]
    public void Late_Register_does_not_reset_existing_history()
    {
        var hb = new MeterHeartbeat();
        hb.Record("Disk", success: true, T0.AddMinutes(5));

        hb.Register("Disk", T0.AddMinutes(30));

        var state = hb.Snapshot()["Disk"];
        Assert.Equal(T0.AddMinutes(5), state.LastSuccessAt);
    }
}
