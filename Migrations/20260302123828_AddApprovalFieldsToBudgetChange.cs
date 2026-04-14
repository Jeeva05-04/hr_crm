using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalFieldsToBudgetChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestDate",
                table: "BudgetChangeRequests");

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                table: "BudgetChangeRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "BudgetChangeRequests",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "BudgetChangeRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "BudgetChangeRequests");

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestDate",
                table: "BudgetChangeRequests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
