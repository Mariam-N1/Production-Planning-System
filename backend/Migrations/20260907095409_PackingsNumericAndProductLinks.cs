using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendProject.Migrations
{
    /// <inheritdoc />
    public partial class PackingsNumericAndProductLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Packings_Size",
                table: "Packings");

            migrationBuilder.AlterColumn<decimal>(
                name: "Size",
                table: "Packings",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "PackingName",
                table: "Packings",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

            migrationBuilder.CreateIndex(
                name: "IX_Packings_PackingName",
                table: "Packings",
                column: "PackingName");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPackings_PackingId",
                table: "ProductPackings",
                column: "PackingId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPackings_ProductId_PackingId",
                table: "ProductPackings",
                columns: new[] { "ProductId", "PackingId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPackings");

            migrationBuilder.DropIndex(
                name: "IX_Packings_PackingName",
                table: "Packings");

            migrationBuilder.AlterColumn<string>(
                name: "Size",
                table: "Packings",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "PackingName",
                table: "Packings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Packings_Size",
                table: "Packings",
                column: "Size");
        }
    }
}
