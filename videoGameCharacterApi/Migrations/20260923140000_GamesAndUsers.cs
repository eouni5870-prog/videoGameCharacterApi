using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace videoGameCharacterApi.Migrations
{
    /// <summary>
    /// Adds the Games table (and links every character to a game), the new character fields and the Users table.
    /// Existing characters are KEPT: every distinct value of the old "Game" text column becomes a row in Games.
    /// </summary>
    public partial class GamesAndUsers : Migration
    {
        // Normalized game name taken from the old text column (empty -> 'Unknown').
        private const string OldGameName =
            "CASE WHEN LTRIM(RTRIM(c.[Game])) = N'' THEN N'Unknown' ELSE LEFT(LTRIM(RTRIM(c.[Game])), 100) END";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) New Games table
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReleaseYear = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            // 2) Copy the existing game names into Games (Genre / ReleaseYear can be edited later via PUT /api/Games/{id})
            migrationBuilder.Sql($@"
INSERT INTO [Games] ([Name], [Genre], [ReleaseYear])
SELECT DISTINCT {OldGameName}, N'Unknown', 2000
FROM [Characters] c;");

            // 3) New character columns
            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "Characters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Characters",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Characters",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Characters",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Characters",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            // 4) Link every existing character to its game
            migrationBuilder.Sql($@"
UPDATE c SET c.[GameId] = g.[Id]
FROM [Characters] c
INNER JOIN [Games] g ON g.[Name] = {OldGameName};");

            // 5) Old text column is no longer needed
            migrationBuilder.DropColumn(
                name: "Game",
                table: "Characters");

            // 6) Length limits (same as the validation on the DTOs)
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Characters",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Characters",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // 7) Users table (for JWT login)
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            // 8) Indexes + foreign key
            migrationBuilder.CreateIndex(
                name: "IX_Characters_GameId",
                table: "Characters",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Name",
                table: "Games",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Games_GameId",
                table: "Characters",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Games_GameId",
                table: "Characters");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Characters_GameId",
                table: "Characters");

            migrationBuilder.AddColumn<string>(
                name: "Game",
                table: "Characters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Put the game name back into the old text column
            migrationBuilder.Sql(@"
UPDATE c SET c.[Game] = g.[Name]
FROM [Characters] c
INNER JOIN [Games] g ON g.[Id] = c.[GameId];");

            migrationBuilder.DropColumn(name: "GameId", table: "Characters");
            migrationBuilder.DropColumn(name: "Description", table: "Characters");
            migrationBuilder.DropColumn(name: "ImageUrl", table: "Characters");
            migrationBuilder.DropColumn(name: "Level", table: "Characters");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "Characters");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Characters",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Characters",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
