using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_identity_provider_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Companies_CompanyId",
                table: "Departments");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyId",
                table: "Departments",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "IdentityProvider",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityProvider", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Companies_CompanyId",
                table: "Departments",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Companies_CompanyId",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "IdentityProvider");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyId",
                table: "Departments",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Companies_CompanyId",
                table: "Departments",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
