using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeOnboardingWorkExperiences");

            migrationBuilder.RenameColumn(
                name: "ParentAadharPaths",
                table: "EmployeeOnboardingDocuments",
                newName: "ParentAadharPath");

            migrationBuilder.RenameColumn(
                name: "EmployeeOnboardingDocumentsId",
                table: "EmployeeOnboardingDocuments",
                newName: "Id");

            migrationBuilder.CreateTable(
                name: "AttendanceTracking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: false),
                    DeviceInfo = table.Column<string>(type: "text", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckOutTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceTracking", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkExperiences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    PreviousCompanyDetails = table.Column<string>(type: "text", nullable: false),
                    OfferedDesignation = table.Column<string>(type: "text", nullable: false),
                    OfferedSalaryNTH = table.Column<decimal>(type: "numeric", nullable: false),
                    OfferedMonthlyCTC = table.Column<decimal>(type: "numeric", nullable: false),
                    OfferedYearlyCTC = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalExperience = table.Column<string>(type: "text", nullable: false),
                    LastCompanyPFNumber = table.Column<string>(type: "text", nullable: false),
                    LastCompanyUAN = table.Column<string>(type: "text", nullable: false),
                    PreviousCompanyPayslipPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkExperiences", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceTracking");

            migrationBuilder.DropTable(
                name: "WorkExperiences");

            migrationBuilder.RenameColumn(
                name: "ParentAadharPath",
                table: "EmployeeOnboardingDocuments",
                newName: "ParentAadharPaths");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "EmployeeOnboardingDocuments",
                newName: "EmployeeOnboardingDocumentsId");

            migrationBuilder.CreateTable(
                name: "EmployeeOnboardingWorkExperiences",
                columns: table => new
                {
                    EmployeeOnboardingWorkExperienceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    LastCompanyPFNumber = table.Column<string>(type: "text", nullable: false),
                    LastCompanyUAN = table.Column<string>(type: "text", nullable: false),
                    OfferedDesignation = table.Column<string>(type: "text", nullable: false),
                    OfferedMonthlyCTC = table.Column<decimal>(type: "numeric", nullable: false),
                    OfferedSalaryNTH = table.Column<decimal>(type: "numeric", nullable: false),
                    OfferedYearlyCTC = table.Column<decimal>(type: "numeric", nullable: false),
                    PreviousCompanyDetails = table.Column<string>(type: "text", nullable: false),
                    PreviousCompanyPayslipPath = table.Column<string>(type: "text", nullable: true),
                    TotalExperience = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeOnboardingWorkExperiences", x => x.EmployeeOnboardingWorkExperienceId);
                });
        }
    }
}
