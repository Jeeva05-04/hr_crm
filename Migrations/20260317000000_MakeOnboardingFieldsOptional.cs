using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class MakeOnboardingFieldsOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Rename WorkExperience table to match current DbSet name ──
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'EmployeeOnboardingWorkExperiences') THEN
                        ALTER TABLE ""EmployeeOnboardingWorkExperiences"" RENAME TO ""WorkExperiences"";
                    END IF;
                END $$;
            ");

            // ── EmployeeOnboardings ──────────────────────────────────

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfJoining",
                table: "EmployeeOnboardings",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "EmployeeOnboardings",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MobileNumber",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BloodGroup",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MaritalStatus",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FatherName",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FatherDOB",
                table: "EmployeeOnboardings",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "IsFatherDeceased",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MotherName",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "MotherDOB",
                table: "EmployeeOnboardings",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "IsMotherDeceased",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PAN",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AadharNumber",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EmergencyContactName",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EmergencyContactRelationship",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TemporaryAddress",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PermanentAddress",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BankName",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "IFSC",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BranchName",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OfficeEmail",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OfficeMobileNumber",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LaptopSerialNumber",
                table: "EmployeeOnboardings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            // ── WorkExperiences ──────────────────────────────────────

            migrationBuilder.AlterColumn<string>(
                name: "PreviousCompanyDetails",
                table: "WorkExperiences",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OfferedDesignation",
                table: "WorkExperiences",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "OfferedSalaryNTH",
                table: "WorkExperiences",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "OfferedMonthlyCTC",
                table: "WorkExperiences",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "OfferedYearlyCTC",
                table: "WorkExperiences",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "TotalExperience",
                table: "WorkExperiences",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LastCompanyPFNumber",
                table: "WorkExperiences",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LastCompanyUAN",
                table: "WorkExperiences",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FullName", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfJoining", table: "EmployeeOnboardings", type: "timestamp with time zone", nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime), oldType: "timestamp with time zone", oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth", table: "EmployeeOnboardings", type: "timestamp with time zone", nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime), oldType: "timestamp with time zone", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MobileNumber", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BloodGroup", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaritalStatus", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FatherName", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FatherDOB", table: "EmployeeOnboardings", type: "timestamp with time zone", nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime), oldType: "timestamp with time zone", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IsFatherDeceased", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MotherName", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "MotherDOB", table: "EmployeeOnboardings", type: "timestamp with time zone", nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime), oldType: "timestamp with time zone", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IsMotherDeceased", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PAN", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AadharNumber", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmergencyContactName", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmergencyContactRelationship", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TemporaryAddress", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PermanentAddress", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BankName", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IFSC", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BranchName", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OfficeEmail", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OfficeMobileNumber", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LaptopSerialNumber", table: "EmployeeOnboardings", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PreviousCompanyDetails", table: "WorkExperiences", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OfferedDesignation", table: "WorkExperiences", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OfferedSalaryNTH", table: "WorkExperiences", type: "numeric", nullable: false,
                defaultValue: 0m, oldClrType: typeof(decimal), oldType: "numeric", oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OfferedMonthlyCTC", table: "WorkExperiences", type: "numeric", nullable: false,
                defaultValue: 0m, oldClrType: typeof(decimal), oldType: "numeric", oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OfferedYearlyCTC", table: "WorkExperiences", type: "numeric", nullable: false,
                defaultValue: 0m, oldClrType: typeof(decimal), oldType: "numeric", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TotalExperience", table: "WorkExperiences", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastCompanyPFNumber", table: "WorkExperiences", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastCompanyUAN", table: "WorkExperiences", type: "text", nullable: false,
                defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);
        }
    }
}
