using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Exceptions;
using videoGameCharacterApi.Services;
using videoGameCharacterApi.Settings;

namespace videoGameCharacterApi.Tests;

public class AuthServiceTests
{
    private static AuthService CreateService(Data.AppDbContext db) => new(
        db,
        Options.Create(new JwtSettings
        {
            Issuer = "test",
            Audience = "test",
            Key = "unit-test-secret-key-that-is-long-enough-123",
            ExpiryMinutes = 5
        }),
        NullLogger<AuthService>.Instance);

    [Fact]
    public async Task Register_ThenLogin_ReturnsToken()
    {
        using var db = TestDb.Create();
        var service = CreateService(db);

        var registered = await service.RegisterAsync(new RegisterRequest { Username = "player1", Password = "secret123" });
        var loggedIn = await service.LoginAsync(new LoginRequest { Username = "PLAYER1", Password = "secret123" });

        Assert.False(string.IsNullOrWhiteSpace(registered.Token));
        Assert.NotNull(loggedIn);
        Assert.False(string.IsNullOrWhiteSpace(loggedIn!.Token));
        Assert.True(loggedIn.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_StoresHashedPassword()
    {
        using var db = TestDb.Create();
        await CreateService(db).RegisterAsync(new RegisterRequest { Username = "player1", Password = "secret123" });

        var user = Assert.Single(db.Users);
        Assert.NotEqual("secret123", user.PasswordHash);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsNull()
    {
        using var db = TestDb.Create();
        var service = CreateService(db);
        await service.RegisterAsync(new RegisterRequest { Username = "player1", Password = "secret123" });

        Assert.Null(await service.LoginAsync(new LoginRequest { Username = "player1", Password = "wrong" }));
        Assert.Null(await service.LoginAsync(new LoginRequest { Username = "nobody", Password = "secret123" }));
    }

    [Fact]
    public async Task Register_WithTakenUsername_ThrowsBadRequest()
    {
        using var db = TestDb.Create();
        var service = CreateService(db);
        await service.RegisterAsync(new RegisterRequest { Username = "player1", Password = "secret123" });

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.RegisterAsync(new RegisterRequest { Username = "Player1", Password = "another123" }));
    }
}
