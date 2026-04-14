using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedRecruitmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedSalary",
                table: "Recruitments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InterviewDate",
                table: "Recruitments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterviewNotes",
                table: "Recruitments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterviewType",
                table: "Recruitments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterviewerName",
                table: "Recruitments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OfferedSalary",
                table: "Recruitments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnboardingId",
                table: "Recruitments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumeUrl",
                table: "Recruitments",
                type: "text",
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedSalary",
                table: "Recruitments");

            migrationBuilder.DropColumn(
                name: "InterviewDate",
                table: "Recruitments");

            migrationBuilder.DropColumn(
                name: "InterviewNotes",
                table: "Recruitments");

            migrationBuilder.DropColumn(
                name: "InterviewType",
                table: "Recruitments");

            migrationBuilder.DropColumn(
                name: "InterviewerName",
                table: "Recruitments");

            migrationBuilder.DropColumn(
                name: "OfferedSalary",
                table: "Recruitments");

            migrationBuilder.DropColumn(
                name: "OnboardingId",
                table: "Recruitments");

            migrationBuilder.DropColumn(
                name: "ResumeUrl",
                table: "Recruitments");

        }
    }
}
