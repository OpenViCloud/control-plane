namespace Titan.Application.Queries.Catalog.VmTemplate.Contracts;

// Customer read. With a zone: the templates synced in that zone — what a VM there can be created from. Without
// one: the templates synced in any zone, for naming the template of a VM that already exists anywhere
// (https://github.com/OpenViCloud/titan/blob/main/docs/features/catalog/design.md#template-customer-read).
public record ListTemplatesQuery(string? ZoneId);
