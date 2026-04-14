using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmployeeName",
                table: "Payrolls",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Payrolls",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "OffBoardings",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Leaves",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "LearningCourses",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "ExitInterviews",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "EmployeeTrainings",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "DigitalSignatures",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Deductions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Allowances",
                newName: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Payrolls",
                newName: "EmployeeName");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Payrolls",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "OffBoardings",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Leaves",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "LearningCourses",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ExitInterviews",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "EmployeeTrainings",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "DigitalSignatures",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Deductions",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Allowances",
                newName: "EmployeeId");
        }
    }
}
