using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendProject.Migrations
{
    /// <inheritdoc />
    public partial class SimpleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPackings");

            migrationBuilder.DropTable(
                name: "ProductShades");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductPackings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackingId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
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
        }
    }
}
