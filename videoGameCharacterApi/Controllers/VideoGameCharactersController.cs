using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Services;

namespace videoGameCharacterApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // create / update / delete need a JWT token
public class VideoGameCharactersController(IVideoGameCharacterService service) : ControllerBase
{
    // GET /api/VideoGameCharacters?game=zelda&role=hero&search=li&page=1&pageSize=10
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<CharacterResponse>>> GetCharacters([FromQuery] CharacterQuery query)
    {
        return Ok(await service.GetCharactersAsync(query));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<CharacterResponse>> GetCharacter(int id)
    {
        var character = await service.GetCharacterByIdAsync(id);

        return character is null
            ? Problem(detail: $"Character with id {id} was not found.", statusCode: StatusCodes.Status404NotFound)
            : Ok(character);
    }

    [HttpPost]
    public async Task<ActionResult<CharacterResponse>> AddCharacter(CreateCharacterRequest character)
    {
        var createdCharacter = await service.AddCharacterAsync(character);

        return CreatedAtAction(nameof(GetCharacter), new { id = createdCharacter.Id }, createdCharacter);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateCharacter(int id, UpdateCharacterRequest character)
    {
        var updated = await service.UpdateCharacterAsync(id, character);

        return updated
            ? NoContent()
            : Problem(detail: $"Character with id {id} was not found.", statusCode: StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteCharacter(int id)
    {
        var deleted = await service.DeleteCharacterAsync(id);

        return deleted
            ? NoContent()
            : Problem(detail: $"Character with id {id} was not found.", statusCode: StatusCodes.Status404NotFound);
    }
}
