using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class fixed_summary_dbquery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SummaryByDayAndEmployee");

            migrationBuilder.DropTable(
                name: "SummaryByDepartments");

            migrationBuilder.DropTable(
                name: "SummaryByEmployees");

            migrationBuilder.DropTable(
                name: "SummaryByWorkstations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SummaryByDayAndEmployee",
                columns: table => new
                {
                    AvgSessionsDuration = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Company = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Department = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Employee = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    SessionsCount = table.Column<int>(type: "int", nullable: false),
                    TotalSessionsDuration = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    WorkstationsCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "SummaryByDepartments",
                columns: table => new
                {
                    AvgSessionsDuration = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    AvgTotalDuartionByEmployee = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    AvgTotalSessionsCountByEmployee = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Company = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Department = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    EmployeesCount = table.Column<int>(type: "int", nullable: false),
                    TotalSessionsCount = table.Column<int>(type: "int", nullable: false),
                    TotalSessionsDuration = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    WorkstationsCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "SummaryByEmployees",
                columns: table => new
                {
                    AvgSessionsCountPerDay = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AvgSessionsDuration = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    AvgWorkingHoursPerDay = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Company = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Department = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Employee = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    TotalSessionsCount = table.Column<int>(type: "int", nullable: false),
                    TotalSessionsDuration = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    WorkingDaysCount = table.Column<int>(type: "int", nullable: false),
                    WorkstationsCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "SummaryByWorkstations",
                columns: table => new
                {
                    AvgSessionsDuration = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    AvgTotalDuartionByEmployee = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    AvgTotalSessionsCountByEmployee = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    EmployeesCount = table.Column<int>(type: "int", nullable: false),
                    TotalSessionsCount = table.Column<int>(type: "int", nullable: false),
                    TotalSessionsDuration = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Workstation = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                });
        }
    }
}
