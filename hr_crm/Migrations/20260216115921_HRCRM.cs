using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace hr_crm.Migrations
{
    /// <inheritdoc />
    public partial class HRCRM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    branch_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    branch_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValueSql: "'Active'::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("branches_pkey", x => x.branch_id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    department_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    department_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    branch_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("departments_pkey", x => x.department_id);
                    table.ForeignKey(
                        name: "fk_department_branch",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "branch_id");
                });

            migrationBuilder.CreateTable(
                name: "knowledge",
                columns: table => new
                {
                    branch_id = table.Column<int>(type: "integer", nullable: false),
                    record_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sub_category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    summary = table.Column<string>(type: "text", nullable: true),
                    approval_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    apporved_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    visibility = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "fk_knowledge_branch",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "branch_id");
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    emergency_contact = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    designation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    date_of_joining = table.Column<DateOnly>(type: "date", nullable: false),
                    salary = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValueSql: "'Active'::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("employees_pkey", x => x.employee_id);
                    table.ForeignKey(
                        name: "fk_department",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "department_id");
                });

            migrationBuilder.CreateTable(
                name: "recruitment",
                columns: table => new
                {
                    candidate_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    applied_position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    application_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("recruitment_pkey", x => x.candidate_id);
                    table.ForeignKey(
                        name: "fk_recruitment_department",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "department_id");
                });

            migrationBuilder.CreateTable(
                name: "attendance",
                columns: table => new
                {
                    attendance_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    attendance_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("attendance_pkey", x => x.attendance_id);
                    table.ForeignKey(
                        name: "fk_employee_attendance",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    project_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    duration = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    manager_id = table.Column<int>(type: "integer", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("projects_pkey", x => x.project_id);
                    table.ForeignKey(
                        name: "fk_project_department",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "department_id");
                    table.ForeignKey(
                        name: "fk_project_manager",
                        column: x => x.manager_id,
                        principalTable: "employees",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateTable(
                name: "todo_tasks",
                columns: table => new
                {
                    task_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    assigned_to = table.Column<int>(type: "integer", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("todo_tasks_pkey", x => x.task_id);
                    table.ForeignKey(
                        name: "fk_todo_employee",
                        column: x => x.assigned_to,
                        principalTable: "employees",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateIndex(
                name: "unique_employee_daily_attendance",
                table: "attendance",
                columns: new[] { "employee_id", "attendance_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departments_branch_id",
                table: "departments",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "employees_email_key",
                table: "employees",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_department_id",
                table: "employees",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_branch_id",
                table: "knowledge",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_department_id",
                table: "projects",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_manager_id",
                table: "projects",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_recruitment_department_id",
                table: "recruitment",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "recruitment_email_key",
                table: "recruitment",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_todo_tasks_assigned_to",
                table: "todo_tasks",
                column: "assigned_to");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance");

            migrationBuilder.DropTable(
                name: "knowledge");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "recruitment");

            migrationBuilder.DropTable(
                name: "todo_tasks");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "branches");
        }
    }
}
