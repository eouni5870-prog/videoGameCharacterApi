using videoGameCharacterApi.Dtos;

namespace videoGameCharacterApi.Services;

public interface IGameService
{
    Task<List<GameResponse>> GetAllGamesAsync();
    Task<GameResponse?> GetGameByIdAsync(int id);
    Task<GameResponse> AddGameAsync(GameRequest game);
    Task<bool> UpdateGameAsync(int id, GameRequest game);
    Task<bool> DeleteGameAsync(int id);
}
