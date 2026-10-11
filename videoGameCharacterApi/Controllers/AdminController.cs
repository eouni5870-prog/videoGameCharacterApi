using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Extensions;
using videoGameCharacterApi.Models;
using videoGameCharacterApi.Services;

namespace videoGameCharacterApi.Controllers;

/// <summary>Admin dashboard: statistics and user management. Admins only.</summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Roles.Admin)]
public class AdminController(IAdminService service) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsResponse>> GetStats()
        => Ok(await service.GetStatsAsync());

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserResponse>>> GetUsers()
        => Ok(await service.GetUsersAsync());

    [HttpPut("users/{id:int}/role")]
    public async Task<ActionResult> SetRole(int id, SetRoleRequest request)
    {
        if (User.GetUserId() is not int currentUserId) return Unauthorized();

        return await service.SetRoleAsync(currentUserId, id, request.Role) switch
        {
            SetRoleStatus.Success => NoContent(),
            SetRoleStatus.UserNotFound => Problem(detail: $"User with id {id} was not found.", statusCode: StatusCodes.Status404NotFound),
            SetRoleStatus.CannotChangeOwnRole => Problem(detail: "You cannot change your own role.", statusCode: StatusCodes.Status400BadRequest),
            _ => Problem(detail: $"Role must be \"{Roles.Admin}\" or \"{Roles.User}\".", statusCode: StatusCodes.Status400BadRequest)
        };
    }
}
