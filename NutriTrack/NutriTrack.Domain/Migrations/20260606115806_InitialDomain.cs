using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriTrack.Domain.Migrations
{
    /// <inheritdoc />
    public partial class InitialDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "domain");

            migrationBuilder.CreateTable(
                name: "Ingredients",
                schema: "domain",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Micronutrients",
                schema: "domain",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DailyReferenceAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Micronutrients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                schema: "domain",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    PrepNote = table.Column<string>(type: "TEXT", nullable: true),
                    YouTubeUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IngredientMicronutrients",
                schema: "domain",
                columns: table => new
                {
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MicronutrientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AmountPer100g = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientMicronutrients", x => new { x.IngredientId, x.MicronutrientId });
                    table.ForeignKey(
                        name: "FK_IngredientMicronutrients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalSchema: "domain",
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IngredientMicronutrients_Micronutrients_MicronutrientId",
                        column: x => x.MicronutrientId,
                        principalSchema: "domain",
                        principalTable: "Micronutrients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealLogs",
                schema: "domain",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EatenAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EatenByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    RecipeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AmountInGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Servings = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealLogs_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalSchema: "domain",
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MealLogs_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "domain",
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RecipeIngredients",
                schema: "domain",
                columns: table => new
                {
                    RecipeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AmountInGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredients", x => new { x.RecipeId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalSchema: "domain",
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "domain",
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientMicronutrients_MicronutrientId",
                schema: "domain",
                table: "IngredientMicronutrients",
                column: "MicronutrientId");

            migrationBuilder.CreateIndex(
                name: "IX_MealLogs_EatenAt",
                schema: "domain",
                table: "MealLogs",
                column: "EatenAt");

            migrationBuilder.CreateIndex(
                name: "IX_MealLogs_EatenByUserId",
                schema: "domain",
                table: "MealLogs",
                column: "EatenByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MealLogs_IngredientId",
                schema: "domain",
                table: "MealLogs",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_MealLogs_RecipeId",
                schema: "domain",
                table: "MealLogs",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_IngredientId",
                schema: "domain",
                table: "RecipeIngredients",
                column: "IngredientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngredientMicronutrients",
                schema: "domain");

            migrationBuilder.DropTable(
                name: "MealLogs",
                schema: "domain");

            migrationBuilder.DropTable(
                name: "RecipeIngredients",
                schema: "domain");

            migrationBuilder.DropTable(
                name: "Micronutrients",
                schema: "domain");

            migrationBuilder.DropTable(
                name: "Ingredients",
                schema: "domain");

            migrationBuilder.DropTable(
                name: "Recipes",
                schema: "domain");
        }
    }
}
