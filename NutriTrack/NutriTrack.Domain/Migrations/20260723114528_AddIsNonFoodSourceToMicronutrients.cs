using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriTrack.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddIsNonFoodSourceToMicronutrients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNonFoodSource",
                schema: "domain",
                table: "Micronutrients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNonFoodSource",
                schema: "domain",
                table: "Micronutrients");
        }
    }
}
