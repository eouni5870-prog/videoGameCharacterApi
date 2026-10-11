using videoGameCharacterApi.Services;

namespace videoGameCharacterApi.Tests;

public class FavoriteServiceTests
{
    [Fact]
    public async Task Add_ThenList_ReturnsTheCharacter()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);
        var user = await TestDb.AddUserAsync(db);
        var mario = await TestDb.AddCharacterAsync(db, game.Id, "Mario");
        var service = new FavoriteService(db);

        Assert.True(await service.AddAsync(user.Id, mario.Id));

        var favorites = await service.GetFavoritesAsync(user.Id);
        Assert.Equal("Mario", Assert.Single(favorites).Name);
        Assert.Equal(new[] { mario.Id }, await service.GetFavoriteIdsAsync(user.Id));
    }

    [Fact]
    public async Task Add_Twice_KeepsOneFavorite()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);
        var user = await TestDb.AddUserAsync(db);
        var mario = await TestDb.AddCharacterAsync(db, game.Id);
        var service = new FavoriteService(db);

        await service.AddAsync(user.Id, mario.Id);
        await service.AddAsync(user.Id, mario.Id);

        Assert.Single(db.Favorites);
    }

    [Fact]
    public async Task Add_UnknownCharacter_ReturnsFalse()
    {
        using var db = TestDb.Create();
        var user = await TestDb.AddUserAsync(db);

        Assert.False(await new FavoriteService(db).AddAsync(user.Id, 999));
    }

    [Fact]
    public async Task Remove_DeletesTheFavorite()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);
        var user = await TestDb.AddUserAsync(db);
        var mario = await TestDb.AddCharacterAsync(db, game.Id);
        var service = new FavoriteService(db);
        await service.AddAsync(user.Id, mario.Id);

        await service.RemoveAsync(user.Id, mario.Id);

        Assert.Empty(db.Favorites);
    }
}
