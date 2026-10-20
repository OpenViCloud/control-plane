namespace Titan.Application.Commands.Billing.TopUp.Contracts;

// https://github.com/OpenViCloud/titan/blob/main/docs/architecture/http-contract.md#create-returns-id: a create answers with the identity it just minted, nothing more.
public record RequestTopUpCommandResult(Guid TopUpId);
