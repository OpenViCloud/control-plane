namespace Titan.Application.Queries.Billing.Wallet.Contracts;

/// OpeningBalance (0 for full history) → the chronological Lines → ClosingBalance (the last line's
/// running balance).
public record WalletStatementQueryResult(
    decimal OpeningBalance, decimal ClosingBalance, string Currency, IReadOnlyList<WalletStatementLine> Lines);
