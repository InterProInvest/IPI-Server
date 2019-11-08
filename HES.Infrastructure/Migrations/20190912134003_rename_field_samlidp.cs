using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class rename_field_samlidp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SamlIdp",
                table: "Employees",
                newName: "SamlIdpDevice");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SamlIdpDevice",
                table: "Employees",
                newName: "SamlIdp");
        }
    }
}
