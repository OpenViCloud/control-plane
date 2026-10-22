namespace Titan.Domain.Aggregates.Platform.ProviderCommand;

/// What enqueuing a key that already has a row does (https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/provider-and-worker.md#provider-command-key).
/// An open row (Pending or Leased) always absorbs the repeat.
public enum ProviderCommandRerun
{
    /// A finished row (Done or Failed) runs again: attach/detach/revert reuse one key per resource.
    AfterAnyFinish,

    /// Only a Failed row runs again; a Done one absorbs the repeat. VM stop/start/delete: a repeat of an intent that
    /// already happened must not happen twice.
    AfterFailureOnly
}
