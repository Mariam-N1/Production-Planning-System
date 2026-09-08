using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackendProject.Migrations
{
    /// <inheritdoc />
    public partial class RelationalSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Products",
                newName: "Discount");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Products",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Packings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Size = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShadeName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductPackings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    PackingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPackings_Packings_PackingId",
                        column: x => x.PackingId,
                        principalTable: "Packings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductPackings_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductShades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ShadeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductShades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductShades_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductShades_Shades_ShadeId",
                        column: x => x.ShadeId,
                        principalTable: "Shades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Title",
                table: "Products",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Packings_Size",
                table: "Packings",
                column: "Size");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPackings_PackingId",
                table: "ProductPackings",
                column: "PackingId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPackings_ProductId_PackingId",
                table: "ProductPackings",
                columns: new[] { "ProductId", "PackingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductShades_ProductId_ShadeId",
                table: "ProductShades",
                columns: new[] { "ProductId", "ShadeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductShades_ShadeId",
                table: "ProductShades",
                column: "ShadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Shades_ShadeName",
                table: "Shades",
                column: "ShadeName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPackings");

            migrationBuilder.DropTable(
                name: "ProductShades");

            migrationBuilder.DropTable(
                name: "Packings");

            migrationBuilder.DropTable(
                name: "Shades");

            migrationBuilder.DropIndex(
                name: "IX_Products_Category",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Title",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Discount",
                table: "Products",
                newName: "Rating");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
