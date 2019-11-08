using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_data_protection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataProtection",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtection", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtection");
        }
    }
}
