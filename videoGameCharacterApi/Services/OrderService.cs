using Microsoft.EntityFrameworkCore;
using videoGameCharacterApi.Data;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Models;

namespace videoGameCharacterApi.Services;

public class OrderService(AppDbContext context, ILogger<OrderService> logger) : IOrderService
{
    public async Task<OrderResult> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        var gameIds = request.GameIds.Distinct().ToList();

        var games = await context.Games.Where(g => gameIds.Contains(g.Id)).ToListAsync();
        if (games.Count != gameIds.Count)
            return new OrderResult(OrderStatus.UnknownGame, Message: "One of the games does not exist any more.");

        var owned = await GetOwnedGameIdsAsync(userId);
        var alreadyOwned = games.Where(g => owned.Contains(g.Id)).Select(g => g.Name).ToList();
        if (alreadyOwned.Count > 0)
            return new OrderResult(OrderStatus.AlreadyOwned,
                Message: $"You already own: {string.Join(", ", alreadyOwned)}.");

        // Prices are taken from the database, never from the client.
        var order = new Order
        {
            UserId = userId,
            Items = games.Select(g => new OrderItem { GameId = g.Id, GameName = g.Name, UnitPrice = g.Price }).ToList(),
            Total = games.Sum(g => g.Price)
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} bought {Count} game(s) for {Total}", userId, games.Count, order.Total);
        return new OrderResult(OrderStatus.Success, ToResponse(order));
    }

    public async Task<List<OrderResponse>> GetMyOrdersAsync(int userId)
    {
        var orders = await context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(ToResponse).ToList();
    }

    public async Task<List<int>> GetOwnedGameIdsAsync(int userId)
        => await context.OrderItems
            .Where(i => i.Order.UserId == userId && i.GameId != null)
            .Select(i => i.GameId!.Value)
            .Distinct()
            .ToListAsync();

    private static OrderResponse ToResponse(Order order) => new()
    {
        Id = order.Id,
        CreatedAt = order.CreatedAt,
        Total = order.Total,
        Items = order.Items
            .Select(i => new OrderItemResponse { GameId = i.GameId, GameName = i.GameName, UnitPrice = i.UnitPrice })
            .ToList()
    };
}
