namespace Titan.Application.Queries.Billing.Wallet.Contracts;

/// One number and its currency (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/money-and-wallet.md#wallet-one-balance). The movement
/// history lives in the statement.
public record WalletQueryResult(decimal Balance, string Currency);
