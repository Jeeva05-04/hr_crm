using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeLocationTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop only if it exists (may not exist in all environments)
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_Attendances_UserId_AttendanceDate"";
            ");

            migrationBuilder.CreateTable(
                name: "EmployeeLocationTrails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttendanceId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLocationTrails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLocationTrails_UserId_RecordedAt",
                table: "EmployeeLocationTrails",
                columns: new[] { "UserId", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeLocationTrails");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Attendances_UserId_AttendanceDate""
                ON ""Attendances"" (""UserId"", ""AttendanceDate"");
            ");
        }
    }
}
