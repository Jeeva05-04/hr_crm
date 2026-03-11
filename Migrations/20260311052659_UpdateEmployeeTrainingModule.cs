using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeTrainingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedBy",
                table: "EmployeeTrainings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "EmployeeTrainings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "EmployeeTrainings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "EmployeeTrainings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationHours",
                table: "EmployeeTrainings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "EmployeeTrainings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCertified",
                table: "EmployeeTrainings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Progress",
                table: "EmployeeTrainings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "EmployeeTrainings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrainingProvider",
                table: "EmployeeTrainings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "EmployeeTrainings",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedBy",
                table: "EmployeeTrainings");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "EmployeeTrainings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "EmployeeTrainings");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "EmployeeTrainings");

            migrationBuilder.DropColumn(
                name: "DurationHours",
                table: "EmployeeTrainings");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "EmployeeTrainings");

            migrationBuilder.DropColumn(
                name: "IsCertified",
                table: "EmployeeTrainings");

            migrationBuilder.DropColumn(
                name: "Progress",
                table: "EmployeeTrainings");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "EmployeeTrainings");

            migrationBuilder.DropColumn(
                name: "TrainingProvider",
                table: "EmployeeTrainings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "EmployeeTrainings");
        }
    }
}
