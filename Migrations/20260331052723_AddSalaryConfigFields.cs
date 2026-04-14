using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryConfigFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payrolls_UserId_Month",
                table: "Payrolls");

            migrationBuilder.AddColumn<decimal>(
                name: "Conveyance",
                table: "SalaryConfigurations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MedicalAllowance",
                table: "SalaryConfigurations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyCTC",
                table: "SalaryConfigurations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Conveyance",
                table: "SalaryConfigurations");

            migrationBuilder.DropColumn(
                name: "MedicalAllowance",
                table: "SalaryConfigurations");

            migrationBuilder.DropColumn(
                name: "MonthlyCTC",
                table: "SalaryConfigurations");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_UserId_Month",
                table: "Payrolls",
                columns: new[] { "UserId", "Month" },
                unique: true);
        }
    }
}
