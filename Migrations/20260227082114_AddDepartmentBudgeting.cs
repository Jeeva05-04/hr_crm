using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentBudgeting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetChangeRequests",
                columns: table => new
                {
                    BudgetChangeRequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetChangeRequests", x => x.BudgetChangeRequestId);
                });

            migrationBuilder.CreateTable(
                name: "BudgetGuidelines",
                columns: table => new
                {
                    BudgetGuidelineId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaxAnnualBudget = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxTrainingPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxResourcePercentage = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetGuidelines", x => x.BudgetGuidelineId);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentBudgets",
                columns: table => new
                {
                    DepartmentBudgetId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    TotalAnnualBudget = table.Column<decimal>(type: "numeric", nullable: false),
                    TrainingBudget = table.Column<decimal>(type: "numeric", nullable: false),
                    ResourceBudget = table.Column<decimal>(type: "numeric", nullable: false),
                    UsedBudget = table.Column<decimal>(type: "numeric", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentBudgets", x => x.DepartmentBudgetId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetChangeRequests");

            migrationBuilder.DropTable(
                name: "BudgetGuidelines");

            migrationBuilder.DropTable(
                name: "DepartmentBudgets");
        }
    }
}
