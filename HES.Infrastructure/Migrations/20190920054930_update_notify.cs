using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class update_notify : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifyId",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Notifications",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "NotifyId",
                table: "Notifications",
                nullable: false,
                defaultValue: 0);
        }
    }
}
