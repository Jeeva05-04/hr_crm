using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    public partial class RemoveAttendanceUniqueConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🔥 Remove unique constraint on (UserId, AttendanceDate)
            migrationBuilder.DropIndex(
                name: "IX_Attendances_UserId_AttendanceDate",
                table: "Attendances");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 🔁 Recreate unique constraint if rolled back
            migrationBuilder.CreateIndex(
                name: "IX_Attendances_UserId_AttendanceDate",
                table: "Attendances",
                columns: new[] { "UserId", "AttendanceDate" },
                unique: true);
        }
    }
}