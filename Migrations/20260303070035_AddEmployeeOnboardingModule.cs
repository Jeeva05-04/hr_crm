using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeOnboardingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeOnboardingDocuments",
                columns: table => new
                {
                    EmployeeOnboardingDocumentsId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    AadharCardPath = table.Column<string>(type: "text", nullable: true),
                    PANCardPath = table.Column<string>(type: "text", nullable: true),
                    BankStatementPath = table.Column<string>(type: "text", nullable: true),
                    BankPassbookPath = table.Column<string>(type: "text", nullable: true),
                    ParentAadharPath = table.Column<string>(type: "text", nullable: true),
                    HighestQualificationDocumentPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeOnboardingDocuments", x => x.EmployeeOnboardingDocumentsId);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeOnboardings",
                columns: table => new
                {
                    EmployeeOnboardingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    DateOfJoining = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    MobileNumber = table.Column<string>(type: "text", nullable: false),
                    BloodGroup = table.Column<string>(type: "text", nullable: false),
                    MaritalStatus = table.Column<string>(type: "text", nullable: false),
                    SpouseName = table.Column<string>(type: "text", nullable: true),
                    SpouseDOB = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ChildrenDetails = table.Column<string>(type: "text", nullable: true),
                    FatherName = table.Column<string>(type: "text", nullable: false),
                    FatherDOB = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsFatherDeceased = table.Column<bool>(type: "boolean", nullable: false),
                    FatherDOD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FatherAge = table.Column<int>(type: "integer", nullable: true),
                    MotherName = table.Column<string>(type: "text", nullable: false),
                    MotherDOB = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsMotherDeceased = table.Column<bool>(type: "boolean", nullable: false),
                    MotherDOD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotherAge = table.Column<int>(type: "integer", nullable: true),
                    PAN = table.Column<string>(type: "text", nullable: false),
                    AadharNumber = table.Column<string>(type: "text", nullable: false),
                    EmergencyContactName = table.Column<string>(type: "text", nullable: false),
                    EmergencyContactRelationship = table.Column<string>(type: "text", nullable: false),
                    TemporaryAddress = table.Column<string>(type: "text", nullable: false),
                    PermanentAddress = table.Column<string>(type: "text", nullable: false),
                    BankName = table.Column<string>(type: "text", nullable: false),
                    AccountNumber = table.Column<string>(type: "text", nullable: false),
                    IFSC = table.Column<string>(type: "text", nullable: false),
                    BranchName = table.Column<string>(type: "text", nullable: false),
                    OfficeEmail = table.Column<string>(type: "text", nullable: false),
                    OfficeMobileNumber = table.Column<string>(type: "text", nullable: false),
                    LaptopSerialNumber = table.Column<string>(type: "text", nullable: false),
                    LaptopImagePath = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeOnboardings", x => x.EmployeeOnboardingId);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeOnboardingWorkExperiences",
                columns: table => new
                {
                    EmployeeOnboardingWorkExperienceId = table.Column<int>(type: "integer", nullable: false)
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
                    table.PrimaryKey("PK_EmployeeOnboardingWorkExperiences", x => x.EmployeeOnboardingWorkExperienceId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeOnboardingDocuments");

            migrationBuilder.DropTable(
                name: "EmployeeOnboardings");

            migrationBuilder.DropTable(
                name: "EmployeeOnboardingWorkExperiences");
        }
    }
}
