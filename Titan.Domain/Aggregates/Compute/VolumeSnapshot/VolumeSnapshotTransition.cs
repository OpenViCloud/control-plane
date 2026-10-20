namespace Titan.Domain.Aggregates.Compute.VolumeSnapshot;

/// The request still waiting for the provider, derived — never stored.
public enum VolumeSnapshotTransition
{
    Creating,
    Deleting
}
