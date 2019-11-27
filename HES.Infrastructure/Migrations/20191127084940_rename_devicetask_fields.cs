using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class rename_devicetask_fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Urls",
                table: "DeviceTasks",
                newName: "OldUrls");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "DeviceTasks",
                newName: "OldName");

            migrationBuilder.RenameColumn(
                name: "Login",
                table: "DeviceTasks",
                newName: "OldLogin");

            migrationBuilder.RenameColumn(
                name: "Apps",
                table: "DeviceTasks",
                newName: "OldApps");

            migrationBuilder.AlterColumn<string>(
                name: "RFID",
                table: "Devices",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OldUrls",
                table: "DeviceTasks",
                newName: "Urls");

            migrationBuilder.RenameColumn(
                name: "OldName",
                table: "DeviceTasks",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "OldLogin",
                table: "DeviceTasks",
                newName: "Login");

            migrationBuilder.RenameColumn(
                name: "OldApps",
                table: "DeviceTasks",
                newName: "Apps");

            migrationBuilder.AlterColumn<string>(
                name: "RFID",
                table: "Devices",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
