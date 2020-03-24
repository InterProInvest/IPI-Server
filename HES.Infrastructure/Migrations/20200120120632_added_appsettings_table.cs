using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_appsettings_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SummaryByDayAndEmployee",
                columns: table => new
                {
                    Date = table.Column<DateTime>(nullable: false),
                    Employee = table.Column<string>(nullable: true),
                    Company = table.Column<string>(nullable: true),
                    Department = table.Column<string>(nullable: true),
                    WorkstationsCount = table.Column<int>(nullable: false),
                    AvgSessionsDuration = table.Column<TimeSpan>(nullable: false),
                    SessionsCount = table.Column<int>(nullable: false),
                    TotalSessionsDuration = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "SummaryByDepartments",
                columns: table => new
                {
                    Company = table.Column<string>(nullable: true),
                    Department = table.Column<string>(nullable: true),
                    EmployeesCount = table.Column<int>(nullable: false),
                    WorkstationsCount = table.Column<int>(nullable: false),
                    TotalSessionsCount = table.Column<int>(nullable: false),
                    TotalSessionsDuration = table.Column<TimeSpan>(nullable: false),
                    AvgSessionsDuration = table.Column<TimeSpan>(nullable: false),
                    AvgTotalDuartionByEmployee = table.Column<TimeSpan>(nullable: false),
                    AvgTotalSessionsCountByEmployee = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "SummaryByEmployees",
                columns: table => new
                {
                    Employee = table.Column<string>(nullable: true),
                    Company = table.Column<string>(nullable: true),
                    Department = table.Column<string>(nullable: true),
                    WorkstationsCount = table.Column<int>(nullable: false),
                    WorkingDaysCount = table.Column<int>(nullable: false),
                    TotalSessionsCount = table.Column<int>(nullable: false),
                    TotalSessionsDuration = table.Column<TimeSpan>(nullable: false),
                    AvgSessionsDuration = table.Column<TimeSpan>(nullable: false),
                    AvgSessionsCountPerDay = table.Column<decimal>(nullable: false),
                    AvgWorkingHoursPerDay = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "SummaryByWorkstations",
                columns: table => new
                {
                    Workstation = table.Column<string>(nullable: true),
                    EmployeesCount = table.Column<int>(nullable: false),
                    TotalSessionsCount = table.Column<int>(nullable: false),
                    TotalSessionsDuration = table.Column<TimeSpan>(nullable: false),
                    AvgSessionsDuration = table.Column<TimeSpan>(nullable: false),
                    AvgTotalDuartionByEmployee = table.Column<TimeSpan>(nullable: false),
                    AvgTotalSessionsCountByEmployee = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "SummaryByDayAndEmployee");

            migrationBuilder.DropTable(
                name: "SummaryByDepartments");

            migrationBuilder.DropTable(
                name: "SummaryByEmployees");

            migrationBuilder.DropTable(
                name: "SummaryByWorkstations");
        }
    }
}
