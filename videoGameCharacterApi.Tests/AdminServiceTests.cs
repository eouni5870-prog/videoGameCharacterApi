using Microsoft.Extensions.Logging.Abstractions;
using videoGameCharacterApi.Data;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Models;
using videoGameCharacterApi.Services;

namespace videoGameCharacterApi.Tests;

public class AdminServiceTests
{
    private static AdminService CreateService(AppDbContext db)
        => new(db, NullLogger<AdminService>.Instance);

    [Fact]
    public async Task GetStats_CountsSalesAndRevenue()
    {
        using var db = TestDb.Create();
        var user = await TestDb.AddUserAsync(db);
        var zelda = await TestDb.AddGameAsync(db, "Zelda");
        var halo = await TestDb.AddGameAsync(db, "Halo");
        zelda.Price = 199m;
        halo.Price = 99m;
        await db.SaveChangesAsync();
        var orders = new OrderService(db, NullLogger<OrderService>.Instance);
        await orders.CreateOrderAsync(user.Id, new CreateOrderRequest { GameIds = [zelda.Id, halo.Id] });

        var stats = await CreateService(db).GetStatsAsync();

        Assert.Equal(1, stats.UserCount);
        Assert.Equal(2, stats.GameCount);
        Assert.Equal(1, stats.OrderCount);
        Assert.Equal(298m, stats.Revenue);
        Assert.Equal("Zelda", stats.TopGames[0].GameName);
        var recent = Assert.Single(stats.RecentOrders);
        Assert.Equal("player1", recent.Username);
        Assert.Equal(2, recent.ItemCount);
    }

    [Fact]
    public async Task SetRole_MakesUserAdmin()
    {
        using var db = TestDb.Create();
        var admin = await TestDb.AddUserAsync(db, "boss");
        var player = await TestDb.AddUserAsync(db, "player1");

        var status = await CreateService(db).SetRoleAsync(admin.Id, player.Id, Roles.Admin);

        Assert.Equal(SetRoleStatus.Success, status);
        Assert.Equal(Roles.Admin, (await db.Users.FindAsync(player.Id))!.Role);
    }

    [Fact]
    public async Task SetRole_OnYourself_IsRefused()
    {
        using var db = TestDb.Create();
        var admin = await TestDb.AddUserAsync(db, "boss");
        admin.Role = Roles.Admin;
        await db.SaveChangesAsync();

        var status = await CreateService(db).SetRoleAsync(admin.Id, admin.Id, Roles.User);

        Assert.Equal(SetRoleStatus.CannotChangeOwnRole, status);
        Assert.Equal(Roles.Admin, admin.Role);
    }

    [Fact]
    public async Task SetRole_WithUnknownRoleOrUser_IsRefused()
    {
        using var db = TestDb.Create();
        var admin = await TestDb.AddUserAsync(db, "boss");
        var player = await TestDb.AddUserAsync(db, "player1");
        var service = CreateService(db);

        Assert.Equal(SetRoleStatus.InvalidRole, await service.SetRoleAsync(admin.Id, player.Id, "SuperAdmin"));
        Assert.Equal(SetRoleStatus.UserNotFound, await service.SetRoleAsync(admin.Id, 999, Roles.Admin));
    }

    [Fact]
    public async Task EnsureAdmin_DoesNothing_WhenAnAdminExists()
    {
        using var db = TestDb.Create();
        var first = await TestDb.AddUserAsync(db, "eya");
        var second = await TestDb.AddUserAsync(db, "player1");
        second.Role = Roles.Admin;
        await db.SaveChangesAsync();

        await SeedData.EnsureAdminAsync(db);

        Assert.Equal(Roles.User, first.Role);
        Assert.Equal(Roles.Admin, second.Role);
    }

    [Fact]
    public async Task EnsureAdmin_MakesFirstAccountAdmin()
    {
        using var db = TestDb.Create();
        var first = await TestDb.AddUserAsync(db, "eya");
        await TestDb.AddUserAsync(db, "player1");

        await SeedData.EnsureAdminAsync(db);

        Assert.Equal(Roles.Admin, first.Role);
        Assert.Equal(1, db.Users.Count(u => u.Role == Roles.Admin));
    }
}
