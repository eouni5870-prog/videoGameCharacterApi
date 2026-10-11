using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Extensions;
using videoGameCharacterApi.Services;

namespace videoGameCharacterApi.Controllers;

/// <summary>The logged-in user's favorite characters.</summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FavoritesController(IFavoriteService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CharacterResponse>>> GetFavorites()
    {
        if (User.GetUserId() is not int userId) return Unauthorized();
        return Ok(await service.GetFavoritesAsync(userId));
    }

    // Only the ids: lets the frontend show the filled hearts on the list.
    [HttpGet("ids")]
    public async Task<ActionResult<List<int>>> GetFavoriteIds()
    {
        if (User.GetUserId() is not int userId) return Unauthorized();
        return Ok(await service.GetFavoriteIdsAsync(userId));
    }

    [HttpPost("{characterId:int}")]
    public async Task<ActionResult> Add(int characterId)
    {
        if (User.GetUserId() is not int userId) return Unauthorized();

        return await service.AddAsync(userId, characterId)
            ? NoContent()
            : Problem(detail: $"Character with id {characterId} was not found.", statusCode: StatusCodes.Status404NotFound);
    }

    [HttpDelete("{characterId:int}")]
    public async Task<ActionResult> Remove(int characterId)
    {
        if (User.GetUserId() is not int userId) return Unauthorized();

        await service.RemoveAsync(userId, characterId);
        return NoContent();
    }
}
