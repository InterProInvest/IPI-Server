using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class rename_deviceaccounts_table_remove_idp_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceAccounts_Employees_EmployeeId",
                table: "DeviceAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceAccounts_SharedAccounts_SharedAccountId",
                table: "DeviceAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceTasks_DeviceAccounts_AccountId",
                table: "DeviceTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_DeviceAccounts_PrimaryAccountId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_DeviceAccounts_DeviceAccountId",
                table: "WorkstationEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationSessions_DeviceAccounts_DeviceAccountId",
                table: "WorkstationSessions");

            migrationBuilder.DropTable(
                name: "SamlIdentityProvider");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceAccounts",
                table: "DeviceAccounts");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "DeviceAccounts",
                newName: "Accounts");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceAccounts_SharedAccountId",
                table: "Accounts",
                newName: "IX_Accounts_SharedAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceAccounts_EmployeeId",
                table: "Accounts",
                newName: "IX_Accounts_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Employees_EmployeeId",
                table: "Accounts",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_SharedAccounts_SharedAccountId",
                table: "Accounts",
                column: "SharedAccountId",
                principalTable: "SharedAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTasks_Accounts_AccountId",
                table: "DeviceTasks",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Accounts_PrimaryAccountId",
                table: "Employees",
                column: "PrimaryAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_Accounts_DeviceAccountId",
                table: "WorkstationEvents",
                column: "DeviceAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationSessions_Accounts_DeviceAccountId",
                table: "WorkstationSessions",
                column: "DeviceAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Employees_EmployeeId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_SharedAccounts_SharedAccountId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceTasks_Accounts_AccountId",
                table: "DeviceTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Accounts_PrimaryAccountId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_Accounts_DeviceAccountId",
                table: "WorkstationEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationSessions_Accounts_DeviceAccountId",
                table: "WorkstationSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts");

            migrationBuilder.RenameTable(
                name: "Accounts",
                newName: "DeviceAccounts");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_SharedAccountId",
                table: "DeviceAccounts",
                newName: "IX_DeviceAccounts_SharedAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_EmployeeId",
                table: "DeviceAccounts",
                newName: "IX_DeviceAccounts_EmployeeId");

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "AspNetUsers",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceAccounts",
                table: "DeviceAccounts",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SamlIdentityProvider",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Url = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SamlIdentityProvider", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceAccounts_Employees_EmployeeId",
                table: "DeviceAccounts",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceAccounts_SharedAccounts_SharedAccountId",
                table: "DeviceAccounts",
                column: "SharedAccountId",
                principalTable: "SharedAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTasks_DeviceAccounts_AccountId",
                table: "DeviceTasks",
                column: "AccountId",
                principalTable: "DeviceAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_DeviceAccounts_PrimaryAccountId",
                table: "Employees",
                column: "PrimaryAccountId",
                principalTable: "DeviceAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_DeviceAccounts_DeviceAccountId",
                table: "WorkstationEvents",
                column: "DeviceAccountId",
                principalTable: "DeviceAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationSessions_DeviceAccounts_DeviceAccountId",
                table: "WorkstationSessions",
                column: "DeviceAccountId",
                principalTable: "DeviceAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
