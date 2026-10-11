using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using videoGameCharacterApi.Data;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Models;
using videoGameCharacterApi.Settings;

namespace videoGameCharacterApi.Services;

public class AuthService(AppDbContext context, IOptions<JwtSettings> jwtOptions, ILogger<AuthService> logger)
    : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;
    private readonly PasswordHasher<User> _hasher = new();

    public const int MinUsernameLength = 3;

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        // Validate the name as it will be stored (the DTO validation runs before trimming).
        var username = request.Username.Trim();
        if (username.Length < MinUsernameLength)
            return new RegisterResult(RegisterStatus.InvalidUsername);

        var lower = username.ToLowerInvariant();
        if (await UsernameExistsAsync(lower))
            return new RegisterResult(RegisterStatus.UsernameTaken);

        var user = new User { Username = username };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        context.Users.Add(user);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Two sign-ups with the same name at the same time: the unique index on Username
            // rejects the second one. Report it as "taken" instead of a 500 error.
            context.Entry(user).State = EntityState.Detached;
            if (await UsernameExistsAsync(lower))
                return new RegisterResult(RegisterStatus.UsernameTaken);
            throw;
        }

        logger.LogInformation("Registered user {Username}", username);
        return new RegisterResult(RegisterStatus.Success, CreateToken(user));
    }

    private Task<bool> UsernameExistsAsync(string lowerUsername)
        => context.Users.AnyAsync(u => u.Username.ToLower() == lowerUsername);

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var lower = request.Username.Trim().ToLowerInvariant();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == lower);
        if (user is null)
            return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Failed login for {Username}", user.Username);
            return null;
        }

        return CreateToken(user);
    }

    private AuthResponse CreateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new AuthResponse
        {
            Username = user.Username,
            Role = user.Role,
            Token = new JsonWebTokenHandler().CreateToken(descriptor),
            ExpiresAt = expiresAt
        };
    }
}
