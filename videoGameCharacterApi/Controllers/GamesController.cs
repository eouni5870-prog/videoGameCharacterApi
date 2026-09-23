using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Services;

namespace videoGameCharacterApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GamesController(IGameService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<GameResponse>>> GetGames()
        => Ok(await service.GetAllGamesAsync());

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<GameResponse>> GetGame(int id)
    {
        var game = await service.GetGameByIdAsync(id);

        return game is null
            ? Problem(detail: $"Game with id {id} was not found.", statusCode: StatusCodes.Status404NotFound)
            : Ok(game);
    }

    [HttpPost]
    public async Task<ActionResult<GameResponse>> AddGame(GameRequest game)
    {
        var created = await service.AddGameAsync(game);
        return CreatedAtAction(nameof(GetGame), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateGame(int id, GameRequest game)
    {
        var updated = await service.UpdateGameAsync(id, game);

        return updated
            ? NoContent()
            : Problem(detail: $"Game with id {id} was not found.", statusCode: StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteGame(int id)
    {
        var deleted = await service.DeleteGameAsync(id);

        return deleted
            ? NoContent()
            : Problem(detail: $"Game with id {id} was not found.", statusCode: StatusCodes.Status404NotFound);
    }
}
