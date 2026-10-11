using videoGameCharacterApi.Dtos;

namespace videoGameCharacterApi.Services;

public enum RegisterStatus
{
    Success,
    UsernameTaken,
    InvalidUsername
}

/// <summary>Result of a sign-up. <see cref="Response"/> is set only when <see cref="Status"/> is Success.</summary>
public record RegisterResult(RegisterStatus Status, AuthResponse? Response = null);

public interface IAuthService
{
    /// <summary>
    /// Creates the account. Expected failures (name already taken, name too short once trimmed)
    /// are returned as a status instead of thrown, so the debugger does not stop on them.
    /// </summary>
    Task<RegisterResult> RegisterAsync(RegisterRequest request);

    /// <summary>Returns null when the username or password is wrong.</summary>
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}
