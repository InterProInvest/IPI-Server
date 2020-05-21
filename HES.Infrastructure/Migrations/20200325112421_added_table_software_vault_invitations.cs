using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_table_software_vault_invitations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SoftwareVaultInvitation",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    EmployeeId = table.Column<string>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ValidTo = table.Column<DateTime>(nullable: false),
                    AcceptedAt = table.Column<DateTime>(nullable: true),
                    SoftwareVaultId = table.Column<string>(nullable: true),
                    ActivationCode = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareVaultInvitation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareVaultInvitation_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareVaultInvitation_SoftwareVaults_SoftwareVaultId",
                        column: x => x.SoftwareVaultId,
                        principalTable: "SoftwareVaults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVaultInvitation_EmployeeId",
                table: "SoftwareVaultInvitation",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVaultInvitation_SoftwareVaultId",
                table: "SoftwareVaultInvitation",
                column: "SoftwareVaultId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SoftwareVaultInvitation");
        }
    }
}
