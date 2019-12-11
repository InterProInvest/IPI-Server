using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_device_license : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceLicenses",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    LicenseOrderId = table.Column<string>(nullable: false),
                    DeviceId = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ImportedAt = table.Column<DateTime>(nullable: false),
                    LicenseEndDate = table.Column<DateTime>(nullable: false),
                    Capabilities = table.Column<int>(nullable: false),
                    AppliedAt = table.Column<DateTime>(nullable: false),
                    Data = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceLicenses", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceLicenses");
        }
    }
}
