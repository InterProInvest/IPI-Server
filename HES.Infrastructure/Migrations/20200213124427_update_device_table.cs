using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class update_device_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceAccessProfiles_AcceessProfileId",
                table: "Devices");

            migrationBuilder.AlterColumn<string>(
                name: "AcceessProfileId",
                table: "Devices",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255) CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceAccessProfiles_AcceessProfileId",
                table: "Devices",
                column: "AcceessProfileId",
                principalTable: "DeviceAccessProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceAccessProfiles_AcceessProfileId",
                table: "Devices");

            migrationBuilder.AlterColumn<string>(
                name: "AcceessProfileId",
                table: "Devices",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceAccessProfiles_AcceessProfileId",
                table: "Devices",
                column: "AcceessProfileId",
                principalTable: "DeviceAccessProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
