using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class fixed_employee_fk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Accounts_PrimaryAccountId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PrimaryAccountId",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryAccountId",
                table: "Employees",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255) CHARACTER SET utf8mb4",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PrimaryAccountId",
                table: "Employees",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PrimaryAccountId",
                table: "Employees",
                column: "PrimaryAccountId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Accounts_PrimaryAccountId",
                table: "Employees",
                column: "PrimaryAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
