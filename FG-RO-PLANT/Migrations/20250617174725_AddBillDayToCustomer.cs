using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FG_RO_PLANT.Migrations
{
    /// <inheritdoc />
    public partial class AddBillDayToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillDay",
                table: "Customers",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillDay",
                table: "Customers");
        }
    }
}
