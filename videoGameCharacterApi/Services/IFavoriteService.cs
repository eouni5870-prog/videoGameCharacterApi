using videoGameCharacterApi.Dtos;

namespace videoGameCharacterApi.Services;

public interface IFavoriteService
{
    Task<List<CharacterResponse>> GetFavoritesAsync(int userId);
    Task<List<int>> GetFavoriteIdsAsync(int userId);

    /// <summary>Returns false when the character does not exist. Adding twice is not an error.</summary>
    Task<bool> AddAsync(int userId, int characterId);

    Task RemoveAsync(int userId, int characterId);
}
