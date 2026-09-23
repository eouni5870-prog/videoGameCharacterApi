using Microsoft.EntityFrameworkCore;
using videoGameCharacterApi.Models;

namespace videoGameCharacterApi.Data;

/// <summary>
/// Starter data. Runs at startup in Development and only inserts
/// when the Games and Characters tables are both empty, so existing data is never touched.
/// </summary>
public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Games.AnyAsync() || await context.Characters.AnyAsync())
            return;

        var mario = new Game { Name = "Super Mario Bros.", Genre = "Platformer", ReleaseYear = 1985 };
        var zelda = new Game { Name = "The Legend of Zelda", Genre = "Action-Adventure", ReleaseYear = 1986 };
        var gow = new Game { Name = "God of War", Genre = "Action", ReleaseYear = 2005 };
        var sf2 = new Game { Name = "Street Fighter II", Genre = "Fighting", ReleaseYear = 1991 };
        var ff7 = new Game { Name = "Final Fantasy VII", Genre = "RPG", ReleaseYear = 1997 };

        context.Characters.AddRange(
            new Character { Name = "Mario", Role = "Hero", Level = 50, Game = mario,
                Description = "A plumber from Brooklyn who saves the Mushroom Kingdom." },
            new Character { Name = "Bowser", Role = "Villain", Level = 60, Game = mario,
                Description = "King of the Koopas who keeps kidnapping Princess Peach." },
            new Character { Name = "Link", Role = "Hero", Level = 55, Game = zelda,
                Description = "A courageous swordsman chosen by the Master Sword." },
            new Character { Name = "Ganondorf", Role = "Villain", Level = 70, Game = zelda,
                Description = "King of evil who seeks the Triforce of Power." },
            new Character { Name = "Zelda", Role = "Support", Level = 45, Game = zelda,
                Description = "Princess of Hyrule who holds the Triforce of Wisdom." },
            new Character { Name = "Kratos", Role = "Hero", Level = 80, Game = gow,
                Description = "A Spartan warrior who wages war on the gods of Olympus." },
            new Character { Name = "Ryu", Role = "Hero", Level = 65, Game = sf2,
                Description = "A wandering martial artist famous for the Hadoken." },
            new Character { Name = "Chun-Li", Role = "Hero", Level = 65, Game = sf2,
                Description = "An Interpol officer with lightning-fast kicks." },
            new Character { Name = "Cloud Strife", Role = "Hero", Level = 75, Game = ff7,
                Description = "An ex-SOLDIER mercenary with a giant Buster Sword." },
            new Character { Name = "Sephiroth", Role = "Villain", Level = 99, Game = ff7,
                Description = "A legendary SOLDIER turned destroyer of the Planet." }
        );

        await context.SaveChangesAsync();
    }
}
