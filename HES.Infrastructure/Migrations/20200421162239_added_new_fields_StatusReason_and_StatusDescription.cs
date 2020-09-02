using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_new_fields_StatusReason_and_StatusDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "Devices");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Devices",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StatusDescription",
                table: "Devices",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusReason",
                table: "Devices",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "StatusDescription",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "StatusReason",
                table: "Devices");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Devices",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
