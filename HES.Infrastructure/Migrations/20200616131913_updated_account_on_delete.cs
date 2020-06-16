using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class updated_account_on_delete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_Accounts_AccountId",
                table: "WorkstationEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationSessions_Accounts_AccountId",
                table: "WorkstationSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_Accounts_AccountId",
                table: "WorkstationEvents",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationSessions_Accounts_AccountId",
                table: "WorkstationSessions",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_Accounts_AccountId",
                table: "WorkstationEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationSessions_Accounts_AccountId",
                table: "WorkstationSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_Accounts_AccountId",
                table: "WorkstationEvents",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationSessions_Accounts_AccountId",
                table: "WorkstationSessions",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
