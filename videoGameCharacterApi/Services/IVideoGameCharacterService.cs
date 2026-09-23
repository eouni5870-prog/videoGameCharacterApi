using videoGameCharacterApi.Dtos;

namespace videoGameCharacterApi.Services;

public interface IVideoGameCharacterService
{
    Task<PagedResult<CharacterResponse>> GetCharactersAsync(CharacterQuery query);
    Task<CharacterResponse?> GetCharacterByIdAsync(int id);
    Task<CharacterResponse> AddCharacterAsync(CreateCharacterRequest character);
    Task<bool> UpdateCharacterAsync(int id, UpdateCharacterRequest character);
    Task<bool> DeleteCharacterAsync(int id);
}
