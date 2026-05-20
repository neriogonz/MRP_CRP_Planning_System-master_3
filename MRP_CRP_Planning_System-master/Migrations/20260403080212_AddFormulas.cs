using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CRP_MRP_Planning_Web.Migrations
{
    /// <inheritdoc />
    public partial class AddFormulas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "formulas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    end_product_id = table.Column<int>(type: "integer", nullable: false),
                    ingredient_id = table.Column<int>(type: "integer", nullable: false),
                    consumption_coeff = table.Column<double>(type: "double precision", nullable: false),
                    work_center_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_formulas", x => x.id);
                    table.ForeignKey(
                        name: "FK_formulas_positions_end_product_id",
                        column: x => x.end_product_id,
                        principalTable: "positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_formulas_positions_ingredient_id",
                        column: x => x.ingredient_id,
                        principalTable: "positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_formulas_work_centers_work_center_id",
                        column: x => x.work_center_id,
                        principalTable: "work_centers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_formulas_end_product_id",
                table: "formulas",
                column: "end_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_formulas_ingredient_id",
                table: "formulas",
                column: "ingredient_id");

            migrationBuilder.CreateIndex(
                name: "IX_formulas_work_center_id",
                table: "formulas",
                column: "work_center_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "formulas");
        }
    }
}
