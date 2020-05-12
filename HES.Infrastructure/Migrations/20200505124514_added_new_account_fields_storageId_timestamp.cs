using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_new_account_fields_storageId_timestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<uint>(
                name: "StorageId",
                table: "Accounts",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "Timestamp",
                table: "Accounts",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.Sql(
            @"
                UPDATE Accounts
                SET StorageId = IdFromDevice;
            ");

            migrationBuilder.DropColumn(
                name: "IdFromDevice",
                table: "Accounts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Accounts");

            migrationBuilder.AddColumn<ushort>(
                name: "IdFromDevice",
                table: "Accounts",
                type: "smallint unsigned",
                nullable: false,
                defaultValue: (ushort)0);
        }
    }
}
