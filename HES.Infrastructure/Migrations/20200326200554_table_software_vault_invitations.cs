using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class table_software_vault_invitations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVaultInvitation_Employees_EmployeeId",
                table: "SoftwareVaultInvitation");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVaultInvitation_SoftwareVaults_SoftwareVaultId",
                table: "SoftwareVaultInvitation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SoftwareVaultInvitation",
                table: "SoftwareVaultInvitation");

            migrationBuilder.RenameTable(
                name: "SoftwareVaultInvitation",
                newName: "SoftwareVaultInvitations");

            migrationBuilder.RenameIndex(
                name: "IX_SoftwareVaultInvitation_SoftwareVaultId",
                table: "SoftwareVaultInvitations",
                newName: "IX_SoftwareVaultInvitations_SoftwareVaultId");

            migrationBuilder.RenameIndex(
                name: "IX_SoftwareVaultInvitation_EmployeeId",
                table: "SoftwareVaultInvitations",
                newName: "IX_SoftwareVaultInvitations_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SoftwareVaultInvitations",
                table: "SoftwareVaultInvitations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVaultInvitations_Employees_EmployeeId",
                table: "SoftwareVaultInvitations",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVaultInvitations_SoftwareVaults_SoftwareVaultId",
                table: "SoftwareVaultInvitations",
                column: "SoftwareVaultId",
                principalTable: "SoftwareVaults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVaultInvitations_Employees_EmployeeId",
                table: "SoftwareVaultInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVaultInvitations_SoftwareVaults_SoftwareVaultId",
                table: "SoftwareVaultInvitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SoftwareVaultInvitations",
                table: "SoftwareVaultInvitations");

            migrationBuilder.RenameTable(
                name: "SoftwareVaultInvitations",
                newName: "SoftwareVaultInvitation");

            migrationBuilder.RenameIndex(
                name: "IX_SoftwareVaultInvitations_SoftwareVaultId",
                table: "SoftwareVaultInvitation",
                newName: "IX_SoftwareVaultInvitation_SoftwareVaultId");

            migrationBuilder.RenameIndex(
                name: "IX_SoftwareVaultInvitations_EmployeeId",
                table: "SoftwareVaultInvitation",
                newName: "IX_SoftwareVaultInvitation_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SoftwareVaultInvitation",
                table: "SoftwareVaultInvitation",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVaultInvitation_Employees_EmployeeId",
                table: "SoftwareVaultInvitation",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVaultInvitation_SoftwareVaults_SoftwareVaultId",
                table: "SoftwareVaultInvitation",
                column: "SoftwareVaultId",
                principalTable: "SoftwareVaults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
