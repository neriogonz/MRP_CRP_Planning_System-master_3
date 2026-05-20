using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRP_MRP_Planning_Web.Migrations
{
    /// <inheritdoc />
    public partial class AddedFieldsToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_work_orders_positions_PositionId",
                table: "work_orders");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                table: "work_orders",
                newName: "position_id");

            migrationBuilder.RenameIndex(
                name: "IX_work_orders_PositionId",
                table: "work_orders",
                newName: "IX_work_orders_position_id");

            migrationBuilder.AddColumn<decimal>(
                name: "height_mm",
                table: "products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preform_type",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "width_mm",
                table: "products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_work_orders_positions_position_id",
                table: "work_orders",
                column: "position_id",
                principalTable: "positions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_work_orders_positions_position_id",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "height_mm",
                table: "products");

            migrationBuilder.DropColumn(
                name: "preform_type",
                table: "products");

            migrationBuilder.DropColumn(
                name: "width_mm",
                table: "products");

            migrationBuilder.RenameColumn(
                name: "position_id",
                table: "work_orders",
                newName: "PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_work_orders_position_id",
                table: "work_orders",
                newName: "IX_work_orders_PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_work_orders_positions_PositionId",
                table: "work_orders",
                column: "PositionId",
                principalTable: "positions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
