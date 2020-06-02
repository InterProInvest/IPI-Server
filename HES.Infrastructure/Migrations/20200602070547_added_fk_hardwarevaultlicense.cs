using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_fk_hardwarevaultlicense : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LicenseOrderId",
                table: "HardwareVaultLicenses",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "HardwareVaultId",
                table: "HardwareVaultLicenses",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_HardwareVaultLicenses_HardwareVaultId",
                table: "HardwareVaultLicenses",
                column: "HardwareVaultId");

            migrationBuilder.CreateIndex(
                name: "IX_HardwareVaultLicenses_LicenseOrderId",
                table: "HardwareVaultLicenses",
                column: "LicenseOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_HardwareVaultLicenses_HardwareVaults_HardwareVaultId",
                table: "HardwareVaultLicenses",
                column: "HardwareVaultId",
                principalTable: "HardwareVaults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HardwareVaultLicenses_LicenseOrders_LicenseOrderId",
                table: "HardwareVaultLicenses",
                column: "LicenseOrderId",
                principalTable: "LicenseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HardwareVaultLicenses_HardwareVaults_HardwareVaultId",
                table: "HardwareVaultLicenses");

            migrationBuilder.DropForeignKey(
                name: "FK_HardwareVaultLicenses_LicenseOrders_LicenseOrderId",
                table: "HardwareVaultLicenses");

            migrationBuilder.DropIndex(
                name: "IX_HardwareVaultLicenses_HardwareVaultId",
                table: "HardwareVaultLicenses");

            migrationBuilder.DropIndex(
                name: "IX_HardwareVaultLicenses_LicenseOrderId",
                table: "HardwareVaultLicenses");

            migrationBuilder.AlterColumn<string>(
                name: "LicenseOrderId",
                table: "HardwareVaultLicenses",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "HardwareVaultId",
                table: "HardwareVaultLicenses",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
