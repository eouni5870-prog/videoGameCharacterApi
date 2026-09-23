using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using videoGameCharacterApi.Data;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Exceptions;
using videoGameCharacterApi.Models;

namespace videoGameCharacterApi.Services;

public class GameService(AppDbContext context, ILogger<GameService> logger) : IGameService
{
    private static readonly Expression<Func<Game, GameResponse>> ToResponse = g => new GameResponse
    {
        Id = g.Id,
        Name = g.Name,
        Genre = g.Genre,
        ReleaseYear = g.ReleaseYear,
        CharacterCount = g.Characters.Count
    };

    public async Task<List<GameResponse>> GetAllGamesAsync()
        => await context.Games.AsNoTracking().OrderBy(g => g.Name).Select(ToResponse).ToListAsync();

    public async Task<GameResponse?> GetGameByIdAsync(int id)
        => await context.Games.AsNoTracking().Where(g => g.Id == id).Select(ToResponse).FirstOrDefaultAsync();

    public async Task<GameResponse> AddGameAsync(GameRequest game)
    {
        var name = game.Name.Trim();
        await EnsureNameIsFreeAsync(name, exceptId: null);

        var newGame = new Game { Name = name, Genre = game.Genre.Trim(), ReleaseYear = game.ReleaseYear };
        context.Games.Add(newGame);
        await context.SaveChangesAsync();

        logger.LogInformation("Created game {GameId} ({Name})", newGame.Id, newGame.Name);
        return (await GetGameByIdAsync(newGame.Id))!;
    }

    public async Task<bool> UpdateGameAsync(int id, GameRequest game)
    {
        var existing = await context.Games.FindAsync(id);
        if (existing is null)
            return false;

        var name = game.Name.Trim();
        await EnsureNameIsFreeAsync(name, exceptId: id);

        existing.Name = name;
        existing.Genre = game.Genre.Trim();
        existing.ReleaseYear = game.ReleaseYear;
        await context.SaveChangesAsync();

        logger.LogInformation("Updated game {GameId}", id);
        return true;
    }

    public async Task<bool> DeleteGameAsync(int id)
    {
        var existing = await context.Games.FindAsync(id);
        if (existing is null)
            return false;

        if (await context.Characters.AnyAsync(c => c.GameId == id))
            throw new BadRequestException("Cannot delete a game that still has characters. Delete or move its characters first.");

        context.Games.Remove(existing);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted game {GameId}", id);
        return true;
    }

    private async Task EnsureNameIsFreeAsync(string name, int? exceptId)
    {
        var lower = name.ToLower();
        if (await context.Games.AnyAsync(g => g.Name.ToLower() == lower && g.Id != exceptId))
            throw new BadRequestException($"A game named '{name}' already exists.");
    }
}
