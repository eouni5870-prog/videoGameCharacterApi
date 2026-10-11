using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Extensions;
using videoGameCharacterApi.Services;

namespace videoGameCharacterApi.Controllers;

/// <summary>Shop: simulated purchases of the logged-in user.</summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersController(IOrderService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetMyOrders()
    {
        if (User.GetUserId() is not int userId) return Unauthorized();
        return Ok(await service.GetMyOrdersAsync(userId));
    }

    [HttpGet("owned-game-ids")]
    public async Task<ActionResult<List<int>>> GetOwnedGameIds()
    {
        if (User.GetUserId() is not int userId) return Unauthorized();
        return Ok(await service.GetOwnedGameIdsAsync(userId));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(CreateOrderRequest request)
    {
        if (User.GetUserId() is not int userId) return Unauthorized();

        var result = await service.CreateOrderAsync(userId, request);
        return result.Status switch
        {
            OrderStatus.Success => Ok(result.Order),
            OrderStatus.AlreadyOwned => Problem(detail: result.Message, statusCode: StatusCodes.Status409Conflict),
            _ => Problem(detail: result.Message, statusCode: StatusCodes.Status400BadRequest)
        };
    }
}
