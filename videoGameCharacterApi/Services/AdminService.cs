using Microsoft.EntityFrameworkCore;
using videoGameCharacterApi.Data;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Models;

namespace videoGameCharacterApi.Services;

public class AdminService(AppDbContext context, ILogger<AdminService> logger) : IAdminService
{
    private const int TopGamesCount = 5;
    private const int RecentOrdersCount = 8;

    public async Task<AdminStatsResponse> GetStatsAsync()
    {
        // Small tables: the sold items are grouped in memory (simple and works with every database).
        var soldItems = await context.OrderItems
            .AsNoTracking()
            .Select(i => new { i.GameName, i.UnitPrice })
            .ToListAsync();

        var topGames = soldItems
            .GroupBy(i => i.GameName)
            .Select(g => new TopGameResponse { GameName = g.Key, Sold = g.Count(), Revenue = g.Sum(i => i.UnitPrice) })
            .OrderByDescending(g => g.Sold)
            .ThenByDescending(g => g.Revenue)
            .Take(TopGamesCount)
            .ToList();

        var recentOrders = await context.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Take(RecentOrdersCount)
            .Select(o => new AdminOrderResponse
            {
                Id = o.Id,
                Username = o.User.Username,
                CreatedAt = o.CreatedAt,
                ItemCount = o.Items.Count,
                Total = o.Total
            })
            .ToListAsync();

        return new AdminStatsResponse
        {
            UserCount = await context.Users.CountAsync(),
            CharacterCount = await context.Characters.CountAsync(),
            GameCount = await context.Games.CountAsync(),
            ReviewCount = await context.Reviews.CountAsync(),
            OrderCount = await context.Orders.CountAsync(),
            Revenue = soldItems.Sum(i => i.UnitPrice),
            TopGames = topGames,
            RecentOrders = recentOrders
        };
    }

    public async Task<List<AdminUserResponse>> GetUsersAsync()
    {
        var users = await context.Users
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .Select(u => new { u.Id, u.Username, u.Role, u.CreatedAt })
            .ToListAsync();

        var orders = await context.Orders
            .AsNoTracking()
            .Select(o => new { o.UserId, o.Total })
            .ToListAsync();

        var reviews = await context.Reviews
            .AsNoTracking()
            .Select(r => r.UserId)
            .ToListAsync();

        return users.Select(u => new AdminUserResponse
        {
            Id = u.Id,
            Username = u.Username,
            Role = u.Role,
            CreatedAt = u.CreatedAt,
            OrderCount = orders.Count(o => o.UserId == u.Id),
            Spent = orders.Where(o => o.UserId == u.Id).Sum(o => o.Total),
            ReviewCount = reviews.Count(id => id == u.Id)
        }).ToList();
    }

    public async Task<SetRoleStatus> SetRoleAsync(int currentUserId, int userId, string role)
    {
        if (!Roles.IsValid(role))
            return SetRoleStatus.InvalidRole;

        if (userId == currentUserId)
            return SetRoleStatus.CannotChangeOwnRole;

        var user = await context.Users.FindAsync(userId);
        if (user is null)
            return SetRoleStatus.UserNotFound;

        user.Role = role;
        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} is now {Role}", userId, role);
        return SetRoleStatus.Success;
    }
}
