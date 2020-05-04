using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_table_software_vaults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SoftwareVaults",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    OS = table.Column<string>(nullable: false),
                    Model = table.Column<string>(nullable: false),
                    ClientAppVersion = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<string>(nullable: false),
                    HasNewLicense = table.Column<bool>(nullable: false),
                    LicenseEndDate = table.Column<DateTime>(nullable: true),
                    LicenseStatus = table.Column<int>(nullable: false),
                    NeedSync = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareVaults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareVaults_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVaults_EmployeeId",
                table: "SoftwareVaults",
                column: "EmployeeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SoftwareVaults");
        }
    }
}
