namespace Titan.Application.Queries.Platform.Legal.Contracts;

// The subject is the caller (Keycloak sub from the request context), so the query carries no parameters.
public record GetConsentStatusQuery();
