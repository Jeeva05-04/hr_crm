using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDepartmentBudgetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DepartmentBudgetId",
                table: "DepartmentBudgets",
                newName: "Id");

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedAmount",
                table: "DepartmentBudgets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "DepartmentBudgets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "FinanceApprovedBy",
                table: "DepartmentBudgets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinanceApprovedDate",
                table: "DepartmentBudgets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeadApprovedBy",
                table: "DepartmentBudgets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HeadApprovedDate",
                table: "DepartmentBudgets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "DepartmentBudgets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentBudgets_DepartmentId",
                table: "DepartmentBudgets",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentBudgets_Departments_DepartmentId",
                table: "DepartmentBudgets",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentBudgets_Departments_DepartmentId",
                table: "DepartmentBudgets");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentBudgets_DepartmentId",
                table: "DepartmentBudgets");

            migrationBuilder.DropColumn(
                name: "ApprovedAmount",
                table: "DepartmentBudgets");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "DepartmentBudgets");

            migrationBuilder.DropColumn(
                name: "FinanceApprovedBy",
                table: "DepartmentBudgets");

            migrationBuilder.DropColumn(
                name: "FinanceApprovedDate",
                table: "DepartmentBudgets");

            migrationBuilder.DropColumn(
                name: "HeadApprovedBy",
                table: "DepartmentBudgets");

            migrationBuilder.DropColumn(
                name: "HeadApprovedDate",
                table: "DepartmentBudgets");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DepartmentBudgets");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DepartmentBudgets",
                newName: "DepartmentBudgetId");
        }
    }
}
