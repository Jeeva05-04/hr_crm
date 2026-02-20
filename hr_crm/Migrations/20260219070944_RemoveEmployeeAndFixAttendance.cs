using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmployeeAndFixAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Employees_EmployeeId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Employees_ManagerId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoTasks_Employees_AssignedToNavigationEmployeeId",
                table: "TodoTasks");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_TodoTasks_AssignedToNavigationEmployeeId",
                table: "TodoTasks");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ManagerId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "AssignedToNavigationEmployeeId",
                table: "TodoTasks");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Attendances",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Attendances_EmployeeId_AttendanceDate",
                table: "Attendances",
                newName: "IX_Attendances_UserId_AttendanceDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Attendances",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Attendances_UserId_AttendanceDate",
                table: "Attendances",
                newName: "IX_Attendances_EmployeeId_AttendanceDate");

            migrationBuilder.AddColumn<int>(
                name: "AssignedToNavigationEmployeeId",
                table: "TodoTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    DateOfJoining = table.Column<DateOnly>(type: "date", nullable: false),
                    Designation = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    EmergencyContact = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Salary = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TodoTasks_AssignedToNavigationEmployeeId",
                table: "TodoTasks",
                column: "AssignedToNavigationEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ManagerId",
                table: "Projects",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Employees_EmployeeId",
                table: "Attendances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Employees_ManagerId",
                table: "Projects",
                column: "ManagerId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoTasks_Employees_AssignedToNavigationEmployeeId",
                table: "TodoTasks",
                column: "AssignedToNavigationEmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
