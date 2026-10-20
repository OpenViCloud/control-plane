namespace Titan.Application.Queries.Billing.Wallet.Contracts;

/// Amount is signed: + for inflow (TopUp/Promo), − for spend. RunningBalance is the balance right after
/// this movement, recorded when the movement happened rather than re-added here.
public record WalletStatementLine(DateTimeOffset At, string Type, decimal Amount, decimal RunningBalance);
