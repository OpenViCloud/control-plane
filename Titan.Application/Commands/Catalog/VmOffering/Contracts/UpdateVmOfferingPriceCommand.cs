namespace Titan.Application.Commands.Catalog.VmOffering.Contracts;

/// Admin price change (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/money-and-wallet.md#price-belongs-to-offering). Only the price is settable — sizing is what the tier IS, and
/// changing it under running VMs would silently re-spec them.
public record UpdateVmOfferingPriceCommand(string VmOfferingId, decimal PricePerHour);
