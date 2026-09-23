using Microsoft.EntityFrameworkCore;
using videoGameCharacterApi.Data;
using videoGameCharacterApi.Models;

namespace videoGameCharacterApi.Tests;

/// <summary>Creates a fresh in-memory database for every test.</summary>
public static class TestDb
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    public static async Task<Game> AddGameAsync(AppDbContext db, string name = "Test Game")
    {
        var game = new Game { Name = name, Genre = "Action", ReleaseYear = 2020 };
        db.Games.Add(game);
        await db.SaveChangesAsync();
        return game;
    }
}
