namespace Titan.Application.Queries.Catalog.Zone.Contracts;

// Zones come from configuration (ZoneOptions), not from a Domain aggregate — hence Zone/ is a folder
// with no aggregate behind it. The result type is the provider port's ZoneInfo.
public record ListZonesQuery();
