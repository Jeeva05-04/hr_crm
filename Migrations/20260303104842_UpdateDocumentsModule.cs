using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDocumentsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ParentAadharPath",
                table: "EmployeeOnboardingDocuments",
                newName: "ParentAadharPaths");

            migrationBuilder.AddColumn<string>(
                name: "AcceptanceLetterPath",
                table: "EmployeeOnboardingDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExperienceLetterPath",
                table: "EmployeeOnboardingDocuments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptanceLetterPath",
                table: "EmployeeOnboardingDocuments");

            migrationBuilder.DropColumn(
                name: "ExperienceLetterPath",
                table: "EmployeeOnboardingDocuments");

            migrationBuilder.RenameColumn(
                name: "ParentAadharPaths",
                table: "EmployeeOnboardingDocuments",
                newName: "ParentAadharPath");
        }
    }
}
