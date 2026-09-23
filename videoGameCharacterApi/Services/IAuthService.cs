using videoGameCharacterApi.Dtos;

namespace videoGameCharacterApi.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>Returns null when the username or password is wrong.</summary>
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}
