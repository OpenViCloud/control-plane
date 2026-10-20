namespace Titan.Application.Commands.Billing.PaymentGatewayTransaction.Contracts;

/// The disposition of one webhook — the endpoint always answers SePay 200 {"success":true} regardless
/// (except auth), so SePay stops retrying; the outcome drives logging/metrics only.
public enum SePayWebhookOutcome { Ignored, Matched, Duplicate, Orphan, AmountMismatch }
