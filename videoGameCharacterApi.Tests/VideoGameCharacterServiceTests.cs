using Microsoft.Extensions.Logging.Abstractions;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Exceptions;
using videoGameCharacterApi.Models;
using videoGameCharacterApi.Services;

namespace videoGameCharacterApi.Tests;

public class VideoGameCharacterServiceTests
{
    private static VideoGameCharacterService CreateService(Data.AppDbContext db)
        => new(db, NullLogger<VideoGameCharacterService>.Instance);

    [Fact]
    public async Task AddCharacter_WithExistingGame_ReturnsCharacterWithGameName()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db, "Zelda");
        var service = CreateService(db);

        var result = await service.AddCharacterAsync(new CreateCharacterRequest
        {
            Name = "Link", Role = "Hero", Level = 10, GameId = game.Id
        });

        Assert.True(result.Id > 0);
        Assert.Equal("Link", result.Name);
        Assert.Equal("Zelda", result.GameName);
        Assert.Equal(10, result.Level);
    }

    [Fact]
    public async Task AddCharacter_WithUnknownGame_ThrowsBadRequest()
    {
        using var db = TestDb.Create();
        var service = CreateService(db);

        await Assert.ThrowsAsync<BadRequestException>(() => service.AddCharacterAsync(new CreateCharacterRequest
        {
            Name = "Nobody", Role = "Hero", GameId = 999
        }));
    }

    [Fact]
    public async Task GetCharacters_FiltersByRole_CaseInsensitive()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);
        db.Characters.AddRange(
            new Character { Name = "A", Role = "Hero", GameId = game.Id },
            new Character { Name = "B", Role = "Villain", GameId = game.Id },
            new Character { Name = "C", Role = "Hero", GameId = game.Id });
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetCharactersAsync(new CharacterQuery { Role = "hero" });

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, c => Assert.Equal("Hero", c.Role));
    }

    [Fact]
    public async Task GetCharacters_FiltersByGameNameAndSearch()
    {
        using var db = TestDb.Create();
        var mario = await TestDb.AddGameAsync(db, "Super Mario");
        var zelda = await TestDb.AddGameAsync(db, "Zelda");
        db.Characters.AddRange(
            new Character { Name = "Mario", Role = "Hero", GameId = mario.Id },
            new Character { Name = "Luigi", Role = "Hero", GameId = mario.Id },
            new Character { Name = "Link", Role = "Hero", GameId = zelda.Id });
        await db.SaveChangesAsync();

        var service = CreateService(db);

        var byGame = await service.GetCharactersAsync(new CharacterQuery { Game = "mario" });
        Assert.Equal(2, byGame.TotalCount);

        var bySearch = await service.GetCharactersAsync(new CharacterQuery { Search = "L" });
        Assert.Equal(new[] { "Luigi", "Link" }, bySearch.Items.Select(c => c.Name));
    }

    [Fact]
    public async Task GetCharacters_Paginates()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);
        for (var i = 1; i <= 25; i++)
            db.Characters.Add(new Character { Name = $"Char {i}", Role = "Hero", GameId = game.Id });
        await db.SaveChangesAsync();

        var page3 = await CreateService(db).GetCharactersAsync(new CharacterQuery { Page = 3, PageSize = 10 });

        Assert.Equal(25, page3.TotalCount);
        Assert.Equal(3, page3.TotalPages);
        Assert.Equal(5, page3.Items.Count);
        Assert.Equal("Char 21", page3.Items[0].Name);
    }

    [Fact]
    public async Task UpdateCharacter_WhenMissing_ReturnsFalse()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);

        var updated = await CreateService(db).UpdateCharacterAsync(123, new UpdateCharacterRequest
        {
            Name = "X", Role = "Hero", GameId = game.Id
        });

        Assert.False(updated);
    }

    [Fact]
    public async Task UpdateCharacter_ChangesFields()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);
        var character = new Character { Name = "Old", Role = "Hero", GameId = game.Id };
        db.Characters.Add(character);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var updated = await service.UpdateCharacterAsync(character.Id, new UpdateCharacterRequest
        {
            Name = "New", Role = "Villain", Level = 42, GameId = game.Id
        });

        Assert.True(updated);
        var reloaded = await service.GetCharacterByIdAsync(character.Id);
        Assert.Equal("New", reloaded!.Name);
        Assert.Equal("Villain", reloaded.Role);
        Assert.Equal(42, reloaded.Level);
    }

    [Fact]
    public async Task DeleteCharacter_RemovesIt()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);
        var character = new Character { Name = "Temp", Role = "Hero", GameId = game.Id };
        db.Characters.Add(character);
        await db.SaveChangesAsync();

        var service = CreateService(db);

        Assert.True(await service.DeleteCharacterAsync(character.Id));
        Assert.Null(await service.GetCharacterByIdAsync(character.Id));
        Assert.False(await service.DeleteCharacterAsync(character.Id));
    }
}
