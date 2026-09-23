using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using videoGameCharacterApi.Data;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Exceptions;
using videoGameCharacterApi.Models;

namespace videoGameCharacterApi.Services
{
    public class VideoGameCharacterService(AppDbContext context, ILogger<VideoGameCharacterService> logger)
        : IVideoGameCharacterService
    {
        // One mapping used by every query, so EF translates it straight to SQL.
        private static readonly Expression<Func<Character, CharacterResponse>> ToResponse = c => new CharacterResponse
        {
            Id = c.Id,
            Name = c.Name,
            Role = c.Role,
            Description = c.Description,
            ImageUrl = c.ImageUrl,
            Level = c.Level,
            CreatedAt = c.CreatedAt,
            GameId = c.GameId,
            GameName = c.Game.Name
        };

        public async Task<PagedResult<CharacterResponse>> GetCharactersAsync(CharacterQuery query)
        {
            var characters = context.Characters.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Game))
            {
                var game = query.Game.Trim().ToLower();
                characters = characters.Where(c => c.Game.Name.ToLower().Contains(game));
            }

            if (query.GameId is not null)
                characters = characters.Where(c => c.GameId == query.GameId);

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                var role = query.Role.Trim().ToLower();
                characters = characters.Where(c => c.Role.ToLower() == role);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                characters = characters.Where(c => c.Name.ToLower().Contains(search));
            }

            var totalCount = await characters.CountAsync();

            var items = await characters
                .OrderBy(c => c.Id)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(ToResponse)
                .ToListAsync();

            return new PagedResult<CharacterResponse>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<CharacterResponse?> GetCharacterByIdAsync(int id)
            => await context.Characters
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync();

        public async Task<CharacterResponse> AddCharacterAsync(CreateCharacterRequest character)
        {
            await EnsureGameExistsAsync(character.GameId);

            var newCharacter = new Character
            {
                Name = character.Name.Trim(),
                Role = character.Role.Trim(),
                Description = character.Description,
                ImageUrl = character.ImageUrl,
                Level = character.Level,
                GameId = character.GameId,
                CreatedAt = DateTime.UtcNow
            };

            context.Characters.Add(newCharacter);
            await context.SaveChangesAsync();

            logger.LogInformation("Created character {CharacterId} ({Name})", newCharacter.Id, newCharacter.Name);

            return (await GetCharacterByIdAsync(newCharacter.Id))!;
        }

        public async Task<bool> UpdateCharacterAsync(int id, UpdateCharacterRequest character)
        {
            var existingCharacter = await context.Characters.FindAsync(id);
            if (existingCharacter is null)
                return false;

            await EnsureGameExistsAsync(character.GameId);

            existingCharacter.Name = character.Name.Trim();
            existingCharacter.Role = character.Role.Trim();
            existingCharacter.Description = character.Description;
            existingCharacter.ImageUrl = character.ImageUrl;
            existingCharacter.Level = character.Level;
            existingCharacter.GameId = character.GameId;

            await context.SaveChangesAsync();

            logger.LogInformation("Updated character {CharacterId}", id);
            return true;
        }

        public async Task<bool> DeleteCharacterAsync(int id)
        {
            var characterToDelete = await context.Characters.FindAsync(id);
            if (characterToDelete is null)
                return false;

            context.Characters.Remove(characterToDelete);
            await context.SaveChangesAsync();

            logger.LogInformation("Deleted character {CharacterId}", id);
            return true;
        }

        private async Task EnsureGameExistsAsync(int gameId)
        {
            if (!await context.Games.AnyAsync(g => g.Id == gameId))
                throw new BadRequestException($"Game with id {gameId} does not exist.");
        }
    }
}
