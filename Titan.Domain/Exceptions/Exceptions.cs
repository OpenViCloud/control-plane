namespace Titan.Domain.Exceptions;

public class NotFoundException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

public class ConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// A SaveChanges lost a concurrency-token or unique-guard race. Infrastructure translates EF's
/// DbUpdateConcurrencyException / unique violation into this so Application can retry or re-read
/// without referencing EF.
public class ConcurrencyConflictException(string code, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public string Code { get; } = code;
}

public class BusinessException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

public class ForbiddenException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

public class BadRequestException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

public class ServiceUnavailableException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
