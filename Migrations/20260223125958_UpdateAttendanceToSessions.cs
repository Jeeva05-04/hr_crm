using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    public partial class UpdateAttendanceToSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ Add new temporary timestamp columns
            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInTime_New",
                table: "Attendances",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOutTime_New",
                table: "Attendances",
                type: "timestamp with time zone",
                nullable: true);

            // 2️⃣ Convert old interval data safely
            migrationBuilder.Sql(@"
                UPDATE ""Attendances""
                SET ""CheckInTime_New"" =
                        ""AttendanceDate"" + ""CheckInTime"",
                    ""CheckOutTime_New"" =
                        CASE
                            WHEN ""CheckOutTime"" IS NOT NULL
                            THEN ""AttendanceDate"" + ""CheckOutTime""
                            ELSE NULL
                        END;
            ");

            // 3️⃣ Drop old columns
            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutTime",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "TotalHours",
                table: "Attendances");

            // 4️⃣ Rename new columns
            migrationBuilder.RenameColumn(
                name: "CheckInTime_New",
                table: "Attendances",
                newName: "CheckInTime");

            migrationBuilder.RenameColumn(
                name: "CheckOutTime_New",
                table: "Attendances",
                newName: "CheckOutTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "CheckInTime",
                table: "Attendances",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "CheckOutTime",
                table: "Attendances",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TotalHours",
                table: "Attendances",
                type: "interval",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutTime",
                table: "Attendances");
        }
    }
}