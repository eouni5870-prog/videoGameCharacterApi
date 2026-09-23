-- Remplit les 3 personnages vides (Id 1, 2, 3) avec des personnages de Super Mario.
-- A lancer une seule fois sur la base VideoGameDb.

UPDATE Games
SET Name = N'Super Mario Bros.', Genre = N'Platformer', ReleaseYear = 1985
WHERE Id = 1;

UPDATE Characters
SET Name = N'Mario', Role = N'Hero', Level = 50,
    Description = N'Le plombier moustachu qui saute sur les ennemis pour sauver le Royaume Champignon.'
WHERE Id = 1;

UPDATE Characters
SET Name = N'Bowser', Role = N'Villain', Level = 70,
    Description = N'Le roi des Koopas, qui essaie toujours d''enlever la princesse Peach.'
WHERE Id = 2;

UPDATE Characters
SET Name = N'Princesse Peach', Role = N'Support', Level = 45,
    Description = N'La princesse du Royaume Champignon, qui aide Mario dans ses aventures.'
WHERE Id = 3;

SELECT c.Id, c.Name, c.Role, c.Level, g.Name AS Game
FROM Characters c JOIN Games g ON g.Id = c.GameId;
