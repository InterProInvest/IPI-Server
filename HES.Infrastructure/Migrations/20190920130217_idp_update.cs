using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class idp_update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SamlIdpDevice",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "SamlIdpDevice",
                table: "Employees",
                nullable: true);
        }
    }
}
