namespace Titan.Application.Queries.Platform.Legal.Contracts;

// Lang is the language of Title and Content: "en" or "vi".
public record LegalDocumentQueryResult(string Key, string Version, string Title, string Content, DateTimeOffset EffectiveAt, string Lang);
