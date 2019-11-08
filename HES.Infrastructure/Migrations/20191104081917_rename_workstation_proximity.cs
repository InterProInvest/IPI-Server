using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class rename_workstation_proximity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkstationProximityDevices");

            migrationBuilder.CreateTable(
                name: "ProximityDevices",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DeviceId = table.Column<string>(nullable: true),
                    WorkstationId = table.Column<string>(nullable: true),
                    LockProximity = table.Column<int>(nullable: false),
                    UnlockProximity = table.Column<int>(nullable: false),
                    LockTimeout = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProximityDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProximityDevices_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProximityDevices_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProximityDevices_DeviceId",
                table: "ProximityDevices",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProximityDevices_WorkstationId",
                table: "ProximityDevices",
                column: "WorkstationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProximityDevices");

            migrationBuilder.CreateTable(
                name: "WorkstationProximityDevices",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DeviceId = table.Column<string>(nullable: true),
                    LockProximity = table.Column<int>(nullable: false),
                    LockTimeout = table.Column<int>(nullable: false),
                    UnlockProximity = table.Column<int>(nullable: false),
                    WorkstationId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkstationProximityDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkstationProximityDevices_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkstationProximityDevices_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationProximityDevices_DeviceId",
                table: "WorkstationProximityDevices",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationProximityDevices_WorkstationId",
                table: "WorkstationProximityDevices",
                column: "WorkstationId");
        }
    }
}
