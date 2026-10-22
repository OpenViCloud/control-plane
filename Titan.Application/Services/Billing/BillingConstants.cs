namespace Titan.Application.Services.Billing;

public static class BillingConstants
{
    /// Titan bills in VND only (https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/money-and-wallet.md#money-is-long-vnd).
    public const string DefaultCurrency = "VND";
}
