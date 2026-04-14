using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CheckInLatitude",
                table: "Attendances",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CheckInLongitude",
                table: "Attendances",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastKnownLatitude",
                table: "Attendances",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastKnownLongitude",
                table: "Attendances",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLocationUpdated",
                table: "Attendances",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInLatitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckInLongitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "LastKnownLatitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "LastKnownLongitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "LastLocationUpdated",
                table: "Attendances");
        }
    }
}
