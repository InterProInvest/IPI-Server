using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_fk_hardwarevaulttask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "HardwareVaultId",
                table: "HardwareVaultTasks",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HardwareVaultTasks_HardwareVaultId",
                table: "HardwareVaultTasks",
                column: "HardwareVaultId");

            migrationBuilder.AddForeignKey(
                name: "FK_HardwareVaultTasks_HardwareVaults_HardwareVaultId",
                table: "HardwareVaultTasks",
                column: "HardwareVaultId",
                principalTable: "HardwareVaults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HardwareVaultTasks_HardwareVaults_HardwareVaultId",
                table: "HardwareVaultTasks");

            migrationBuilder.DropIndex(
                name: "IX_HardwareVaultTasks_HardwareVaultId",
                table: "HardwareVaultTasks");

            migrationBuilder.AlterColumn<string>(
                name: "HardwareVaultId",
                table: "HardwareVaultTasks",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
