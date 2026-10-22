namespace Titan.Application.Commands.Networking.Network.Contracts;

// https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/http-contract.md#create-returns-id: a create answers with the identity it just minted, nothing more.
public record CreateNetworkCommandResult(Guid NetworkId);
