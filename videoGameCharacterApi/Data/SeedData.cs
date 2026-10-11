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

    /// <summary>
    /// One-time fix for the 3 old characters that were created without a name / role.
    /// Only touches characters 1, 2 and 3 when their name is still empty or "EYA", so it runs only once.
    /// </summary>
    public static async Task FixOldCharactersAsync(AppDbContext context)
    {
        var old = await context.Characters
            .Where(c => c.Id <= 3 && (c.Name == "" || c.Name == "EYA"))
            .ToListAsync();
        if (old.Count == 0)
            return;

        var game = await context.Games.FindAsync(1);
        if (game is not null)
        {
            game.Name = "Super Mario Bros.";
            game.Genre = "Platformer";
            game.ReleaseYear = 1985;
        }

        var data = new Dictionary<int, (string Name, string Role, int Level, string Description)>
        {
            [1] = ("Mario", "Hero", 50, "Le plombier moustachu qui saute sur les ennemis pour sauver le Royaume Champignon."),
            [2] = ("Bowser", "Villain", 70, "Le roi des Koopas, qui essaie toujours d'enlever la princesse Peach."),
            [3] = ("Princesse Peach", "Support", 45, "La princesse du Royaume Champignon, qui aide Mario dans ses aventures."),
        };

        foreach (var c in old)
        {
            if (!data.TryGetValue(c.Id, out var d)) continue;
            c.Name = d.Name;
            c.Role = d.Role;
            c.Level = d.Level;
            c.Description = d.Description;
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Adds a bigger collection of games and characters (Development only).
    /// Safe to run at every start: a game or character that already exists (same name) is skipped,
    /// so nothing the user created or edited is changed.
    /// </summary>
    public static async Task AddMoreCharactersAsync(AppDbContext context)
    {
        var games = new (string Name, string Genre, int Year)[]
        {
            ("Super Mario Bros.", "Platformer", 1985),
            ("The Legend of Zelda", "Action-Adventure", 1986),
            ("God of War", "Action", 2005),
            ("Street Fighter II", "Fighting", 1991),
            ("Final Fantasy VII", "RPG", 1997),
            ("Sonic the Hedgehog", "Platformer", 1991),
            ("Pokémon Rouge et Bleu", "RPG", 1996),
            ("Halo: Combat Evolved", "FPS", 2001),
            ("Tomb Raider", "Action-Adventure", 1996),
            ("Metal Gear Solid", "Infiltration", 1998),
            ("The Witcher 3", "RPG", 2015),
            ("Minecraft", "Sandbox", 2011),
        };

        var gameIds = new Dictionary<string, int>();
        foreach (var (name, genre, year) in games)
        {
            var lower = name.ToLower();
            var game = await context.Games.FirstOrDefaultAsync(g => g.Name.ToLower() == lower);
            if (game is null)
            {
                game = new Game { Name = name, Genre = genre, ReleaseYear = year };
                context.Games.Add(game);
                await context.SaveChangesAsync();
            }
            gameIds[name] = game.Id;
        }

        var characters = new (string Name, string Role, int Level, string Game, string Description)[]
        {
            ("Luigi", "Hero", 45, "Super Mario Bros.", "Le frère de Mario, grand, peureux mais toujours présent quand il le faut."),
            ("Yoshi", "Support", 40, "Super Mario Bros.", "Un petit dinosaure vert qui avale ses ennemis et transporte Mario."),
            ("Toad", "Support", 25, "Super Mario Bros.", "Fidèle serviteur de la princesse, toujours plein d'énergie."),
            ("Wario", "Villain", 55, "Super Mario Bros.", "Le rival jaloux de Mario, obsédé par l'or."),
            ("Link", "Hero", 70, "The Legend of Zelda", "Un jeune guerrier silencieux armé d'une épée légendaire."),
            ("Zelda", "Support", 60, "The Legend of Zelda", "Princesse d'Hyrule, sage et dotée de pouvoirs magiques."),
            ("Ganondorf", "Villain", 85, "The Legend of Zelda", "Le seigneur des ténèbres qui veut dominer Hyrule."),
            ("Kratos", "Hero", 92, "God of War", "Un guerrier spartiate d'une force immense, en guerre contre les dieux."),
            ("Atreus", "Support", 50, "God of War", "Le fils de Kratos, archer agile et curieux."),
            ("Ryu", "Hero", 75, "Street Fighter II", "Un combattant discret qui parcourt le monde pour devenir plus fort."),
            ("Chun-Li", "Hero", 74, "Street Fighter II", "Une experte en arts martiaux réputée pour ses coups de pied rapides."),
            ("M. Bison", "Villain", 82, "Street Fighter II", "Le chef d'une organisation criminelle aux pouvoirs psychiques."),
            ("Cloud Strife", "Hero", 80, "Final Fantasy VII", "Un mercenaire à l'épée géante, au passé mystérieux."),
            ("Tifa Lockhart", "Support", 70, "Final Fantasy VII", "Une combattante au grand cœur, amie d'enfance de Cloud."),
            ("Sephiroth", "Villain", 99, "Final Fantasy VII", "Un ancien soldat d'élite devenu une menace pour la planète."),
            ("Sonic", "Hero", 72, "Sonic the Hedgehog", "Un hérisson bleu capable de courir plus vite que le son."),
            ("Tails", "Support", 45, "Sonic the Hedgehog", "Un jeune renard à deux queues, génie de la mécanique."),
            ("Dr. Eggman", "Villain", 65, "Sonic the Hedgehog", "Un savant fou qui construit des robots pour conquérir le monde."),
            ("Pikachu", "Hero", 55, "Pokémon Rouge et Bleu", "Une petite créature électrique, compagnon fidèle des dresseurs."),
            ("Dracaufeu", "Hero", 78, "Pokémon Rouge et Bleu", "Un dragon de feu puissant et fier."),
            ("Mewtwo", "Villain", 95, "Pokémon Rouge et Bleu", "Une créature créée en laboratoire, aux pouvoirs psychiques redoutables."),
            ("Master Chief", "Hero", 88, "Halo: Combat Evolved", "Un super-soldat en armure qui protège l'humanité."),
            ("Cortana", "Support", 68, "Halo: Combat Evolved", "Une intelligence artificielle qui guide Master Chief."),
            ("Lara Croft", "Hero", 76, "Tomb Raider", "Une archéologue aventurière qui explore des tombeaux oubliés."),
            ("Solid Snake", "Hero", 84, "Metal Gear Solid", "Un soldat expert en infiltration, discret et méthodique."),
            ("Liquid Snake", "Villain", 83, "Metal Gear Solid", "Le frère jumeau de Snake, chef d'un groupe rebelle."),
            ("Geralt de Riv", "Hero", 87, "The Witcher 3", "Un chasseur de monstres aux réflexes surhumains."),
            ("Yennefer", "Support", 81, "The Witcher 3", "Une magicienne puissante au caractère bien trempé."),
            ("Steve", "Hero", 30, "Minecraft", "Un aventurier qui construit, mine et survit dans un monde de blocs."),
            ("Creeper", "Villain", 35, "Minecraft", "Une créature verte silencieuse qui explose quand on s'approche."),
        };

        foreach (var (name, role, level, gameName, description) in characters)
        {
            var lower = name.ToLower();
            if (await context.Characters.AnyAsync(c => c.Name.ToLower() == lower))
                continue;

            context.Characters.Add(new Character
            {
                Name = name,
                Role = role,
                Level = level,
                Description = description,
                GameId = gameIds[gameName]
            });
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Characters created before the stats existed have 0 everywhere: give them stats based on
    /// their role and level. Characters that already have stats are not touched.
    /// </summary>
    public static async Task FillMissingStatsAsync(AppDbContext context)
    {
        var withoutStats = await context.Characters
            .Where(c => c.Strength == 0 && c.Speed == 0 && c.Magic == 0 && c.Defense == 0)
            .ToListAsync();
        if (withoutStats.Count == 0)
            return;

        foreach (var c in withoutStats)
        {
            // Small variation per character so two characters with the same role don't look identical
            var v = c.Name.Sum(ch => (int)ch) % 15; // deterministic (string.GetHashCode changes at every run)
            var level = Math.Clamp(c.Level, 1, 100);

            var (strength, speed, magic, defense) = c.Role.ToLower() switch
            {
                "hero" => (level + 5, level, level - 15, level - 5),
                "villain" => (level + 10, level - 10, level + 5, level),
                "support" => (level - 15, level + 5, level + 10, level - 5),
                _ => (level, level, level, level)
            };

            c.Strength = Math.Clamp(strength + v - 7, 5, 100);
            c.Speed = Math.Clamp(speed - v + 7, 5, 100);
            c.Magic = Math.Clamp(magic + (v % 5) * 2 - 4, 5, 100);
            c.Defense = Math.Clamp(defense - (v % 7) + 3, 5, 100);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Gives a shop price to the games that don't have one yet (price 0).
    /// A game whose price was set by the user is never changed.
    /// </summary>
    public static async Task SetMissingPricesAsync(AppDbContext context)
    {
        // Games without a price, or still with an old price in euros (19.99 ... 69.99),
        // get a price in Tunisian dinars.
        var games = await context.Games.ToListAsync();
        var toUpdate = games
            .Where(g => g.Price == 0 || (g.Price < 70 && g.Price % 1 == 0.99m))
            .ToList();
        if (toUpdate.Count == 0)
            return;

        foreach (var game in toUpdate)
        {
            // Newer games cost more: between 59 DT and 229 DT, ending with 9 like a real shop.
            var age = Math.Clamp(DateTime.UtcNow.Year - game.ReleaseYear, 0, 40);
            var dinars = Math.Clamp(230 - age * 6m, 60m, 230m);
            game.Price = Math.Floor(dinars / 10) * 10 - 1;
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// When nobody is admin yet, the first account ever created becomes admin.
    /// Other admins can then be named from the admin page.
    /// </summary>
    public static async Task EnsureAdminAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Role == Roles.Admin))
            return;

        var first = await context.Users.OrderBy(u => u.Id).FirstOrDefaultAsync();
        if (first is null)
            return;

        first.Role = Roles.Admin;
        await context.SaveChangesAsync();
    }
}
