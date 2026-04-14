using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ConveyanceAllowance",
                table: "Payrolls",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DOJ",
                table: "Payrolls",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Payrolls",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Designation",
                table: "Payrolls",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "EmployeePF",
                table: "Payrolls",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerPF",
                table: "Payrolls",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentType",
                table: "Payrolls",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "GrossSalary",
                table: "Payrolls",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HRA",
                table: "Payrolls",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MedicalAllowance",
                table: "Payrolls",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyCTC",
                table: "Payrolls",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyCTCApportioned",
                table: "Payrolls",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "NoOfPayableDays",
                table: "Payrolls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherAllowance",
                table: "Payrolls",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PT",
                table: "Payrolls",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TAOrPBonus",
                table: "Payrolls",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConveyanceAllowance",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "DOJ",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "Designation",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "EmployeePF",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "EmployerPF",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "GrossSalary",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "HRA",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "MedicalAllowance",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "MonthlyCTC",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "MonthlyCTCApportioned",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "NoOfPayableDays",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "OtherAllowance",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "PT",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "TAOrPBonus",
                table: "Payrolls");
        }
    }
}
