using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class AddDigitalSignatureFilePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DigitalSignatures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignedFilePath",
                table: "DigitalSignatures",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DigitalSignatures");

            migrationBuilder.DropColumn(
                name: "SignedFilePath",
                table: "DigitalSignatures");
        }
    }
}
