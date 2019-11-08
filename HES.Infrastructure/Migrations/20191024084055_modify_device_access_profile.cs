using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class modify_device_access_profile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ButtonExpiration",
                table: "DeviceAccessProfiles");

            migrationBuilder.DropColumn(
                name: "ButtonNewLink",
                table: "DeviceAccessProfiles");

            migrationBuilder.DropColumn(
                name: "MasterKeyExpiration",
                table: "DeviceAccessProfiles");

            migrationBuilder.DropColumn(
                name: "MasterKeyNewLink",
                table: "DeviceAccessProfiles");

            migrationBuilder.DropColumn(
                name: "PinNewLink",
                table: "DeviceAccessProfiles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ButtonExpiration",
                table: "DeviceAccessProfiles",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ButtonNewLink",
                table: "DeviceAccessProfiles",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MasterKeyExpiration",
                table: "DeviceAccessProfiles",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "MasterKeyNewLink",
                table: "DeviceAccessProfiles",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PinNewLink",
                table: "DeviceAccessProfiles",
                nullable: false,
                defaultValue: false);
        }
    }
}
