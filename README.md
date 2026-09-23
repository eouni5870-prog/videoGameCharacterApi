# GameHeroes – API (videoGameCharacterApi)

API REST en **ASP.NET Core (.NET 10)** pour gérer une collection de personnages de jeux vidéo.
Elle est utilisée par le frontend React : [video-game-frontend](https://github.com/eouni5870-prog/video-game-frontend).

## Fonctionnalités

- CRUD des **personnages** et des **jeux** (relation un jeu → plusieurs personnages)
- **Recherche**, **filtres** (jeu, rôle) et **pagination**
- **Authentification JWT** (inscription / connexion), mots de passe hachés
- **Upload de photos** pour les personnages
- **Validation** des données et **gestion globale des erreurs** (ProblemDetails)
- **16 tests unitaires** (xUnit) et **intégration continue** (GitHub Actions)

## Technologies

C# · ASP.NET Core Web API · Entity Framework Core · SQL Server LocalDB · JWT · Scalar / OpenAPI · xUnit · GitHub Actions

## Architecture

```
videoGameCharacterApi/
├── Controllers/   → routes HTTP (VideoGameCharacters, Games, Auth, Uploads)
├── Services/      → logique métier (interfaces + implémentations)
├── Data/          → AppDbContext (EF Core) et données de départ
├── Models/        → entités : Character, Game, User
├── Dtos/          → objets échangés avec le client (+ validation)
├── Middleware/    → GlobalExceptionHandler
├── Migrations/    → historique de la base de données
└── Program.cs     → configuration (services, JWT, CORS, base)
videoGameCharacterApi.Tests/ → tests unitaires
```

## Principales routes

| Méthode | Route | Accès |
|---|---|---|
| GET | `/api/VideoGameCharacters?search=&role=&gameId=&page=&pageSize=` | Public |
| GET | `/api/VideoGameCharacters/{id}` | Public |
| POST / PUT / DELETE | `/api/VideoGameCharacters/{id}` | Connecté |
| GET | `/api/Games` | Public |
| POST / PUT / DELETE | `/api/Games/{id}` | Connecté |
| POST | `/api/Auth/register`, `/api/Auth/login` | Public |
| POST | `/api/Uploads` | Connecté |

## Lancer le projet

1. Ouvrir `videoGameCharacterApi.slnx` dans Visual Studio.
2. Première fois : `Update-Database` dans la Console du Gestionnaire de package.
3. Lancer avec **▶ https** (ou `dotnet run --launch-profile https`).
4. Documentation interactive : https://localhost:7062/scalar

## Auteur

Eya Ouni – 3ème année Génie Logiciel
