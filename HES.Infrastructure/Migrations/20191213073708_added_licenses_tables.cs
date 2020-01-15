using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_licenses_tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Capabilities",
                table: "DeviceLicenses");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DeviceLicenses");

            migrationBuilder.DropColumn(
                name: "LicenseEndDate",
                table: "DeviceLicenses");

            migrationBuilder.AddColumn<DateTime>(
                name: "LicenseEndDate",
                table: "Devices",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LicenseStatus",
                table: "Devices",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseOrderId",
                table: "DeviceLicenses",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ImportedAt",
                table: "DeviceLicenses",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "DeviceLicenses",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "DeviceLicenses",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicenseEndDate",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LicenseStatus",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "DeviceLicenses");

            migrationBuilder.AlterColumn<string>(
                name: "LicenseOrderId",
                table: "DeviceLicenses",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<DateTime>(
                name: "ImportedAt",
                table: "DeviceLicenses",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "DeviceLicenses",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "Capabilities",
                table: "DeviceLicenses",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DeviceLicenses",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LicenseEndDate",
                table: "DeviceLicenses",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
