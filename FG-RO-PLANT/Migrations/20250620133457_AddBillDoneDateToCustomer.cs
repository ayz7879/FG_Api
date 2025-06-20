using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FG_RO_PLANT.Migrations
{
    /// <inheritdoc />
    public partial class AddBillDoneDateToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "BillDoneDate",
                table: "Customers",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillDoneDate",
                table: "Customers");
        }
    }
}
