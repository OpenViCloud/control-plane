namespace Titan.Application.Options;

/// <summary>Liveness of the worker's reconcilers, measured by their polls of GET /internal/reconcile/*.</summary>
public class ReconcilerLivenessOptions
{
    public const string SectionName = "ReconcilerLiveness";

    /// <summary>
    /// A reconciler that has not asked for its list for this long is reported stale. 10: the worker ticks every
    /// 2 minutes, so a few missed ticks are tolerated and a stopped worker shows within minutes.
    /// </summary>
    public int StaleMinutes { get; set; } = 10;
}
