using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDocumentUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentUrl",
                table: "DigitalSignatures");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentUrl",
                table: "DigitalSignatures",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
