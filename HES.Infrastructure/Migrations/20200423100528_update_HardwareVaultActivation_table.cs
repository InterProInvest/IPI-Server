using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class update_HardwareVaultActivation_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "HardwareVaultActivations");

            migrationBuilder.DropColumn(
                name: "LastWrongAttempt",
                table: "HardwareVaultActivations");

            migrationBuilder.DropColumn(
                name: "WrongAttemptsCount",
                table: "HardwareVaultActivations");

            migrationBuilder.AddColumn<string>(
                name: "VaultId",
                table: "HardwareVaultActivations",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VaultId",
                table: "HardwareVaultActivations");

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "HardwareVaultActivations",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWrongAttempt",
                table: "HardwareVaultActivations",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WrongAttemptsCount",
                table: "HardwareVaultActivations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
