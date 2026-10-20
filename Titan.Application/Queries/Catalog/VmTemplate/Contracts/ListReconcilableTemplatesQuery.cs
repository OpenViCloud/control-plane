namespace Titan.Application.Queries.Catalog.VmTemplate.Contracts;

// The template reconciler of one zone asks which image aliases to observe on that zone's cluster.
public record ListReconcilableTemplatesQuery(string ZoneId);
