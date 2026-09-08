using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendProject.Migrations
{
    /// <inheritdoc />
    public partial class ProductShadesAndUniqueHex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "Shades",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

            migrationBuilder.CreateIndex(
                name: "IX_Shades_ColorCode",
                table: "Shades",
                column: "ColorCode",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductShades");

            migrationBuilder.DropIndex(
                name: "IX_Shades_ColorCode",
                table: "Shades");

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "Shades",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
