using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class updated_workstation_on_delete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_Workstations_WorkstationId",
                table: "WorkstationEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationProximityVaults_Workstations_WorkstationId",
                table: "WorkstationProximityVaults");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationSessions_Workstations_WorkstationId",
                table: "WorkstationSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_Workstations_WorkstationId",
                table: "WorkstationEvents",
                column: "WorkstationId",
                principalTable: "Workstations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationProximityVaults_Workstations_WorkstationId",
                table: "WorkstationProximityVaults",
                column: "WorkstationId",
                principalTable: "Workstations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationSessions_Workstations_WorkstationId",
                table: "WorkstationSessions",
                column: "WorkstationId",
                principalTable: "Workstations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_Workstations_WorkstationId",
                table: "WorkstationEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationProximityVaults_Workstations_WorkstationId",
                table: "WorkstationProximityVaults");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationSessions_Workstations_WorkstationId",
                table: "WorkstationSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_Workstations_WorkstationId",
                table: "WorkstationEvents",
                column: "WorkstationId",
                principalTable: "Workstations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationProximityVaults_Workstations_WorkstationId",
                table: "WorkstationProximityVaults",
                column: "WorkstationId",
                principalTable: "Workstations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationSessions_Workstations_WorkstationId",
                table: "WorkstationSessions",
                column: "WorkstationId",
                principalTable: "Workstations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
