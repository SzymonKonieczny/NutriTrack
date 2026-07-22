using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriTrack.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOwnedRecipesAndIngredients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Note: SQLite ignores schemas, but we keep them for future SQL Server migration.

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                schema: "domain",
                table: "Recipes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                schema: "domain",
                table: "Recipes",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Private");

            // Existing recipes were all admin-created - set them to Public so they remain visible to everyone.
            migrationBuilder.Sql("UPDATE \"Recipes\" SET \"Visibility\" = 'Public'");

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                schema: "domain",
                table: "Ingredients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                schema: "domain",
                table: "Ingredients",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Private");

            // Existing ingredients were all admin-created — set them to Public so they remain visible to everyone.
            migrationBuilder.Sql("UPDATE \"Ingredients\" SET \"Visibility\" = 'Public'");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_AuthorId",
                schema: "domain",
                table: "Recipes",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_AuthorId",
                schema: "domain",
                table: "Ingredients",
                column: "AuthorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recipes_AuthorId",
                schema: "domain",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_AuthorId",
                schema: "domain",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                schema: "domain",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Visibility",
                schema: "domain",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                schema: "domain",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Visibility",
                schema: "domain",
                table: "Ingredients");
        }
    }
}
