using Microsoft.Extensions.Logging.Abstractions;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Services;

namespace videoGameCharacterApi.Tests;

public class ReviewServiceTests
{
    private static ReviewService CreateService(Data.AppDbContext db)
        => new(db, NullLogger<ReviewService>.Instance);

    [Fact]
    public async Task Save_Twice_UpdatesTheSameReview()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);
        var user = await TestDb.AddUserAsync(db, "eya");
        var mario = await TestDb.AddCharacterAsync(db, game.Id);
        var service = CreateService(db);

        await service.SaveAsync(user.Id, mario.Id, new ReviewRequest { Rating = 3, Comment = "Pas mal" });
        var updated = await service.SaveAsync(user.Id, mario.Id, new ReviewRequest { Rating = 5, Comment = "  Génial  " });

        Assert.NotNull(updated);
        Assert.Equal(5, updated!.Rating);
        Assert.Equal("Génial", updated.Comment);
        Assert.Equal("eya", updated.Username);
        Assert.Single(db.Reviews);
    }

    [Fact]
    public async Task Save_UnknownCharacter_ReturnsNull()
    {
        using var db = TestDb.Create();
        var user = await TestDb.AddUserAsync(db);

        Assert.Null(await CreateService(db).SaveAsync(user.Id, 999, new ReviewRequest { Rating = 4 }));
    }

    [Fact]
    public async Task CharacterResponse_ContainsAverageRatingAndCounts()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);
        var eya = await TestDb.AddUserAsync(db, "eya");
        var sam = await TestDb.AddUserAsync(db, "sam");
        var mario = await TestDb.AddCharacterAsync(db, game.Id);
        var reviews = CreateService(db);
        await reviews.SaveAsync(eya.Id, mario.Id, new ReviewRequest { Rating = 5 });
        await reviews.SaveAsync(sam.Id, mario.Id, new ReviewRequest { Rating = 2 });
        await new FavoriteService(db).AddAsync(eya.Id, mario.Id);

        var characters = new VideoGameCharacterService(db, NullLogger<VideoGameCharacterService>.Instance);
        var response = await characters.GetCharacterByIdAsync(mario.Id);

        Assert.Equal(3.5, response!.AverageRating);
        Assert.Equal(2, response.ReviewCount);
        Assert.Equal(1, response.FavoriteCount);
    }

    [Fact]
    public async Task Delete_RemovesOnlyMyReview()
    {
        using var db = TestDb.Create();
        var game = await TestDb.AddGameAsync(db);
        var eya = await TestDb.AddUserAsync(db, "eya");
        var sam = await TestDb.AddUserAsync(db, "sam");
        var mario = await TestDb.AddCharacterAsync(db, game.Id);
        var service = CreateService(db);
        await service.SaveAsync(eya.Id, mario.Id, new ReviewRequest { Rating = 5 });
        await service.SaveAsync(sam.Id, mario.Id, new ReviewRequest { Rating = 2 });

        Assert.True(await service.DeleteAsync(eya.Id, mario.Id));
        Assert.False(await service.DeleteAsync(eya.Id, mario.Id));
        Assert.Equal("sam", Assert.Single(await service.GetForCharacterAsync(mario.Id)).Username);
    }
}
