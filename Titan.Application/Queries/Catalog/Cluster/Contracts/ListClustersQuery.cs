namespace Titan.Application.Queries.Catalog.Cluster.Contracts;

// Clusters are read live from the compute provider for one configured zone — no Domain aggregate
// behind this folder. The result type is the provider port's ClusterInfo.
public record ListClustersQuery(string ZoneId);
