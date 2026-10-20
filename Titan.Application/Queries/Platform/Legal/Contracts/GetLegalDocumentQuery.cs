namespace Titan.Application.Queries.Platform.Legal.Contracts;

// There is one gated document; the query only picks its language ("vi", otherwise English).
public record GetLegalDocumentQuery(string? Lang);
