using Microsoft.EntityFrameworkCore;
using videoGameCharacterApi.Data;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Models;

namespace videoGameCharacterApi.Services;

public class FavoriteService(AppDbContext context) : IFavoriteService
{
    public async Task<List<CharacterResponse>> GetFavoritesAsync(int userId)
        => await context.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.Character)
            .Select(VideoGameCharacterService.ToResponse)
            .ToListAsync();

    public async Task<List<int>> GetFavoriteIdsAsync(int userId)
        => await context.Favorites
            .Where(f => f.UserId == userId)
            .Select(f => f.CharacterId)
            .ToListAsync();

    public async Task<bool> AddAsync(int userId, int characterId)
    {
        if (!await context.Characters.AnyAsync(c => c.Id == characterId))
            return false;

        if (await context.Favorites.AnyAsync(f => f.UserId == userId && f.CharacterId == characterId))
            return true;

        context.Favorites.Add(new Favorite { UserId = userId, CharacterId = characterId });
        await context.SaveChangesAsync();
        return true;
    }

    public async Task RemoveAsync(int userId, int characterId)
    {
        var favorite = await context.Favorites.FindAsync(userId, characterId);
        if (favorite is null)
            return;

        context.Favorites.Remove(favorite);
        await context.SaveChangesAsync();
    }
}
