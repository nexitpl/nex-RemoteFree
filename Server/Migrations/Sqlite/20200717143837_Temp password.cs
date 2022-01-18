using Microsoft.EntityFrameworkCore.Migrations;

namespace nexRemoteFree.Server.Migrations.Sqlite
{
    public partial class Temppassword : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TempPassword",
                table: "nexRemoteFreeUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TempPassword",
                table: "nexRemoteFreeUsers");
        }
    }
}
