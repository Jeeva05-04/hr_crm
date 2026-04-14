using System;
using hr_crm.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable

namespace hr_crm.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260403103141_AddEmployeeConversionFlow")]
    public class AddEmployeeConversionFlow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConvertedEmployeeId",
                table: "EmployeeOnboardings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConvertedAt",
                table: "EmployeeOnboardings",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConvertedEmployeeId",
                table: "EmployeeOnboardings");

            migrationBuilder.DropColumn(
                name: "ConvertedAt",
                table: "EmployeeOnboardings");
        }
    }
}
