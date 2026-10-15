using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using videoGameCharacterApi.Dtos;
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

        Assert.Equal(RegisterStatus.Success, registered.Status);
        Assert.False(string.IsNullOrWhiteSpace(registered.Response!.Token));
        Assert.NotNull(loggedIn);
        Assert.False(string.IsNullOrWhiteSpace(loggedIn!.Token));
        Assert.True(loggedIn.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_FirstAccount_IsAdmin()
    {
        using var db = TestDb.Create();

        var result = await CreateService(db).RegisterAsync(new RegisterRequest { Username = "eya", Password = "secret123" });

        Assert.Equal(Models.Roles.Admin, result.Response!.Role);
    }

    [Fact]
    public async Task Register_NewAccount_IsNormalUser()
    {
        using var db = TestDb.Create();
        var admin = await TestDb.AddUserAsync(db, "boss");
        admin.Role = Models.Roles.Admin;
        await db.SaveChangesAsync();

        var result = await CreateService(db).RegisterAsync(new RegisterRequest { Username = "player1", Password = "secret123" });

        Assert.Equal(Models.Roles.User, result.Response!.Role);
        Assert.Equal(Models.Roles.User, db.Users.Single(u => u.Username == "player1").Role);
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
    public async Task Register_WithTakenUsername_ReturnsUsernameTaken()
    {
        using var db = TestDb.Create();
        var service = CreateService(db);
        await service.RegisterAsync(new RegisterRequest { Username = "player1", Password = "secret123" });

        var result = await service.RegisterAsync(new RegisterRequest { Username = "Player1", Password = "another123" });

        Assert.Equal(RegisterStatus.UsernameTaken, result.Status);
        Assert.Null(result.Response);
        Assert.Single(db.Users);
    }

    [Fact]
    public async Task Register_WithTooShortTrimmedUsername_ReturnsInvalidUsername()
    {
        using var db = TestDb.Create();

        var result = await CreateService(db).RegisterAsync(new RegisterRequest { Username = "  ab  ", Password = "secret123" });

        Assert.Equal(RegisterStatus.InvalidUsername, result.Status);
        Assert.Empty(db.Users);
    }

    [Fact]
    public async Task Register_TrimsUsername()
    {
        using var db = TestDb.Create();

        await CreateService(db).RegisterAsync(new RegisterRequest { Username = "  player1  ", Password = "secret123" });

        Assert.Equal("player1", Assert.Single(db.Users).Username);
    }
}
