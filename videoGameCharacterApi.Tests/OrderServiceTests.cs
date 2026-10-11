using Microsoft.Extensions.Logging.Abstractions;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Services;

namespace videoGameCharacterApi.Tests;

public class OrderServiceTests
{
    private static OrderService CreateService(Data.AppDbContext db)
        => new(db, NullLogger<OrderService>.Instance);

    [Fact]
    public async Task CreateOrder_UsesDatabasePrices_AndComputesTotal()
    {
        using var db = TestDb.Create();
        var user = await TestDb.AddUserAsync(db);
        var zelda = await TestDb.AddGameAsync(db, "Zelda");
        var halo = await TestDb.AddGameAsync(db, "Halo");
        zelda.Price = 29.99m;
        halo.Price = 19.99m;
        await db.SaveChangesAsync();

        var result = await CreateService(db).CreateOrderAsync(user.Id, new CreateOrderRequest { GameIds = [zelda.Id, halo.Id, zelda.Id] });

        Assert.Equal(OrderStatus.Success, result.Status);
        Assert.Equal(49.98m, result.Order!.Total);
        Assert.Equal(2, result.Order.Items.Count);
    }

    [Fact]
    public async Task CreateOrder_GameAlreadyOwned_IsRefused()
    {
        using var db = TestDb.Create();
        var user = await TestDb.AddUserAsync(db);
        var zelda = await TestDb.AddGameAsync(db, "Zelda");
        var service = CreateService(db);
        await service.CreateOrderAsync(user.Id, new CreateOrderRequest { GameIds = [zelda.Id] });

        var again = await service.CreateOrderAsync(user.Id, new CreateOrderRequest { GameIds = [zelda.Id] });

        Assert.Equal(OrderStatus.AlreadyOwned, again.Status);
        Assert.Single(db.Orders);
    }

    [Fact]
    public async Task CreateOrder_UnknownGame_IsRefused()
    {
        using var db = TestDb.Create();
        var user = await TestDb.AddUserAsync(db);

        var result = await CreateService(db).CreateOrderAsync(user.Id, new CreateOrderRequest { GameIds = [999] });

        Assert.Equal(OrderStatus.UnknownGame, result.Status);
        Assert.Empty(db.Orders);
    }

    [Fact]
    public async Task OwnedGames_AndOrders_BelongToTheUser()
    {
        using var db = TestDb.Create();
        var eya = await TestDb.AddUserAsync(db, "eya");
        var sam = await TestDb.AddUserAsync(db, "sam");
        var zelda = await TestDb.AddGameAsync(db, "Zelda");
        var service = CreateService(db);
        await service.CreateOrderAsync(eya.Id, new CreateOrderRequest { GameIds = [zelda.Id] });

        Assert.Equal(new[] { zelda.Id }, await service.GetOwnedGameIdsAsync(eya.Id));
        Assert.Empty(await service.GetOwnedGameIdsAsync(sam.Id));
        Assert.Single(await service.GetMyOrdersAsync(eya.Id));
    }
}
