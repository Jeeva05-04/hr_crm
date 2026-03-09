using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountDeactivationColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AccountDeactivation",
                table: "OffBoardings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountDeactivation",
                table: "OffBoardings");
        }
    }
}
