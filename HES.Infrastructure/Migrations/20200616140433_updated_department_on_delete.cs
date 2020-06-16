using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class updated_department_on_delete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_Departments_DepartmentId",
                table: "WorkstationEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Workstations_Departments_DepartmentId",
                table: "Workstations");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationSessions_Departments_DepartmentId",
                table: "WorkstationSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_Departments_DepartmentId",
                table: "WorkstationEvents",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Workstations_Departments_DepartmentId",
                table: "Workstations",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationSessions_Departments_DepartmentId",
                table: "WorkstationSessions",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_Departments_DepartmentId",
                table: "WorkstationEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Workstations_Departments_DepartmentId",
                table: "Workstations");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationSessions_Departments_DepartmentId",
                table: "WorkstationSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_Departments_DepartmentId",
                table: "WorkstationEvents",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workstations_Departments_DepartmentId",
                table: "Workstations",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationSessions_Departments_DepartmentId",
                table: "WorkstationSessions",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
