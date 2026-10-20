namespace Titan.Application.Queries.Platform.Legal.Contracts;

public record ConsentStatusQueryResult(bool MustAcceptTerms, string DocumentKey, string DocumentVersion);
