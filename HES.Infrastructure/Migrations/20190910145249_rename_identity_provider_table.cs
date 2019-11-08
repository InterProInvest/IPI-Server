using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class rename_identity_provider_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentityProvider");

            migrationBuilder.CreateTable(
                name: "SamlIdentityProvider",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SamlIdentityProvider", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SamlIdentityProvider");

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
        }
    }
}
