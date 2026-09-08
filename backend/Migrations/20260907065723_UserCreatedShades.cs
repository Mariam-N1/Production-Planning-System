using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackendProject.Migrations
{
    /// <inheritdoc />
    public partial class UserCreatedShades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Packings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Packings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Packings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Packings",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Packings",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Packings",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Shades",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Shades",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Shades",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Shades",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Shades",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Shades",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Shades",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Shades",
                keyColumn: "Id",
                keyValue: 8);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Packings",
                columns: new[] { "Id", "Size", "Weight" },
                values: new object[,]
                {
                    { 1, "1-Quart", 1.1m },
                    { 2, "1-Gallon", 4.5m },
                    { 3, "2-Gallon", 9.0m },
                    { 4, "5-Gallon", 22.5m },
                    { 5, "500ml Can", 0.6m },
                    { 6, "20L Drum", 90.0m }
                });

            migrationBuilder.InsertData(
                table: "Shades",
                columns: new[] { "Id", "ColorCode", "ShadeName" },
                values: new object[,]
                {
                    { 1, "#0B6E4F", "Emerald Green" },
                    { 2, "#1A1A1A", "Matte Black" },
                    { 3, "#F7F3E9", "Ivory White" },
                    { 4, "#1B4FA0", "Cobalt Blue" },
                    { 5, "#B4552C", "Terracotta" },
                    { 6, "#5A6570", "Slate Grey" },
                    { 7, "#E8B531", "Sunflower" },
                    { 8, "#D9899A", "Rose Blush" }
                });
        }
    }
}
