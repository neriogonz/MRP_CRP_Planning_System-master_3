using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRP_MRP_Planning_Web.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewFieldsToRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_positions",
                columns: table => new
                {
                    products_id = table.Column<int>(type: "integer", nullable: false),
                    positions_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_positions", x => new { x.products_id, x.positions_id });
                    table.ForeignKey(
                        name: "FK_product_positions_positions_positions_id",
                        column: x => x.positions_id,
                        principalTable: "positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_positions_products_products_id",
                        column: x => x.products_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_positions_positions_id",
                table: "product_positions",
                column: "positions_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_positions");
        }
    }
}
