using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using videoGameCharacterApi.Data;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Exceptions;
using videoGameCharacterApi.Models;
using videoGameCharacterApi.Settings;

namespace videoGameCharacterApi.Services;

public class AuthService(AppDbContext context, IOptions<JwtSettings> jwtOptions, ILogger<AuthService> logger)
    : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;
    private readonly PasswordHasher<User> _hasher = new();

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var username = request.Username.Trim();
        var lower = username.ToLower();

        if (await context.Users.AnyAsync(u => u.Username.ToLower() == lower))
            throw new BadRequestException("Username is already taken.");

        var user = new User { Username = username };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        logger.LogInformation("Registered user {Username}", username);
        return CreateToken(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var lower = request.Username.Trim().ToLower();
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
                new Claim(ClaimTypes.Name, user.Username)
            }),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new AuthResponse
        {
            Username = user.Username,
            Token = new JsonWebTokenHandler().CreateToken(descriptor),
            ExpiresAt = expiresAt
        };
    }
}
