namespace videoGameCharacterApi.Exceptions;

/// <summary>Thrown when a request breaks a business rule. Mapped to HTTP 400.</summary>
public class BadRequestException(string message) : Exception(message);
