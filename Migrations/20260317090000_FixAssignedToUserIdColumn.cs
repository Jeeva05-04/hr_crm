using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class FixAssignedToUserIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add AssignedToUserId column to Recruitments if it doesn't already exist
            migrationBuilder.Sql(@"
                ALTER TABLE ""Recruitments""
                ADD COLUMN IF NOT EXISTS ""AssignedToUserId"" integer;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Recruitments""
                DROP COLUMN IF EXISTS ""AssignedToUserId"";
            ");
        }
    }
}
