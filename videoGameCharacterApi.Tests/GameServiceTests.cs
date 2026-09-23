using Microsoft.Extensions.Logging.Abstractions;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Exceptions;
using videoGameCharacterApi.Models;
using videoGameCharacterApi.Services;

namespace videoGameCharacterApi.Tests;

public class GameServiceTests
{
    private static GameService CreateService(Data.AppDbContext db)
        => new(db, NullLogger<GameService>.Instance);

    [Fact]
    public async Task AddGame_WithDuplicateName_ThrowsBadRequest()
    {
        using var db = TestDb.Create();
        await TestDb.AddGameAsync(db, "Halo");

        await Assert.ThrowsAsync<BadRequestException>(() => CreateService(db).AddGameAsync(
            new GameRequest { Name = "halo", Genre = "Shooter", ReleaseYear = 2001 }));
    }

    [Fact]
    public async Task DeleteGame_WithCharacters_ThrowsBadRequest()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);
        db.Characters.Add(new Character { Name = "Hero", Role = "Hero", GameId = game.Id });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() => CreateService(db).DeleteGameAsync(game.Id));
    }

    [Fact]
    public async Task DeleteGame_WithoutCharacters_Works()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);
        var service = CreateService(db);

        Assert.True(await service.DeleteGameAsync(game.Id));
        Assert.Null(await service.GetGameByIdAsync(game.Id));
    }

    [Fact]
    public async Task GetGames_ReturnsCharacterCount()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);
        db.Characters.AddRange(
            new Character { Name = "A", Role = "Hero", GameId = game.Id },
            new Character { Name = "B", Role = "Hero", GameId = game.Id });
        await db.SaveChangesAsync();

        var games = await CreateService(db).GetAllGamesAsync();

        Assert.Equal(2, Assert.Single(games).CharacterCount);
    }
}
