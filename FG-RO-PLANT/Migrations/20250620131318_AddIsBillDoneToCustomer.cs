using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FG_RO_PLANT.Migrations
{
    /// <inheritdoc />
    public partial class AddIsBillDoneToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBillDone",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBillDone",
                table: "Customers");
        }
    }
}
