namespace Titan.Application.Commands.Billing.PaymentGatewayTransaction.Contracts;

/// The fields of one inbound SePay webhook this handler uses (a subset of the SePay payload).
/// TransferType is "in"/"out"; only "in" adds credit. Content/Code carry the transfer memo where the
/// TITAN-XXXXXXXX PaymentRef appears.
public record ReceiveSePayPaymentCommand(
    string Id, string Gateway, DateTimeOffset TransactionDate, string TransferType,
    decimal TransferAmount, string? Content, string? Code);
