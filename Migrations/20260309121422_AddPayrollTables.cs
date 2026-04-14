using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allowances",
                table: "Payrolls");

            migrationBuilder.RenameColumn(
                name: "PayrollMonth",
                table: "Payrolls",
                newName: "Month");

            migrationBuilder.RenameColumn(
                name: "GrossSalary",
                table: "Payrolls",
                newName: "TotalDeductions");

            migrationBuilder.RenameColumn(
                name: "Deductions",
                table: "Payrolls",
                newName: "TotalAllowances");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Payrolls",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Payrolls",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Payrolls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Allowances",
                columns: table => new
                {
                    AllowanceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    AllowanceType = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Month = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allowances", x => x.AllowanceId);
                });

            migrationBuilder.CreateTable(
                name: "Deductions",
                columns: table => new
                {
                    DeductionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    DeductionType = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Month = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deductions", x => x.DeductionId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Allowances");

            migrationBuilder.DropTable(
                name: "Deductions");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Payrolls");

            migrationBuilder.RenameColumn(
                name: "TotalDeductions",
                table: "Payrolls",
                newName: "GrossSalary");

            migrationBuilder.RenameColumn(
                name: "TotalAllowances",
                table: "Payrolls",
                newName: "Deductions");

            migrationBuilder.RenameColumn(
                name: "Month",
                table: "Payrolls",
                newName: "PayrollMonth");

            migrationBuilder.AddColumn<decimal>(
                name: "Allowances",
                table: "Payrolls",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
