using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class move_to_new_accounts_scheme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceAccounts_Devices_DeviceId",
                table: "DeviceAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceTasks_DeviceAccounts_DeviceAccountId",
                table: "DeviceTasks");

            migrationBuilder.DropIndex(
                name: "IX_DeviceTasks_DeviceAccountId",
                table: "DeviceTasks");

            migrationBuilder.DropIndex(
                name: "IX_DeviceAccounts_DeviceId",
                table: "DeviceAccounts");

            migrationBuilder.DropColumn(
                name: "DeviceAccountId",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "OldApps",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "OldLogin",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "OldName",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "OldUrls",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "PrimaryAccountId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "DeviceAccounts");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "DeviceAccounts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DeviceAccounts");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryAccountId",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "DeviceTasks",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NeedSync",
                table: "Devices",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PrimaryAccountId",
                table: "Employees",
                column: "PrimaryAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTasks_AccountId",
                table: "DeviceTasks",
                column: "AccountId");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceTasks_DeviceAccounts_AccountId",
                table: "DeviceTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_DeviceAccounts_PrimaryAccountId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PrimaryAccountId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_DeviceTasks_AccountId",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "PrimaryAccountId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "NeedSync",
                table: "Devices");

            migrationBuilder.AddColumn<string>(
                name: "DeviceAccountId",
                table: "DeviceTasks",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldApps",
                table: "DeviceTasks",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldLogin",
                table: "DeviceTasks",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldName",
                table: "DeviceTasks",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldUrls",
                table: "DeviceTasks",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryAccountId",
                table: "Devices",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "DeviceAccounts",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "DeviceAccounts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DeviceAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTasks_DeviceAccountId",
                table: "DeviceTasks",
                column: "DeviceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAccounts_DeviceId",
                table: "DeviceAccounts",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceAccounts_Devices_DeviceId",
                table: "DeviceAccounts",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTasks_DeviceAccounts_DeviceAccountId",
                table: "DeviceTasks",
                column: "DeviceAccountId",
                principalTable: "DeviceAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
