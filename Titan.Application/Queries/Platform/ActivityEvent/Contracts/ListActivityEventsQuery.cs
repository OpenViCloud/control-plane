namespace Titan.Application.Queries.Platform.ActivityEvent.Contracts;

// Limit <= 0 means "not given" and falls back to the default; the handler clamps it.
public record ListActivityEventsQuery(int Limit);
