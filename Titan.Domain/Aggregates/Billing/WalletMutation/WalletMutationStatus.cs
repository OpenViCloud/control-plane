namespace Titan.Domain.Aggregates.Billing.WalletMutation;

/// https://github.com/OpenViCloud/titan/blob/main/docs/architecture/money-and-wallet.md#single-writer-outbox — Pending is written in the caller's transaction;
/// the single-writer consumer moves it to Applied in its own. Failed: the wallet could not cover it when applied,
/// or the sweeper gave up on it as poison.
public enum WalletMutationStatus
{
    Pending,
    Applied,
    Failed
}
