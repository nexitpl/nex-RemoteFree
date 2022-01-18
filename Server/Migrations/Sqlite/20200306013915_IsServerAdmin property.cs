using Microsoft.EntityFrameworkCore.Migrations;

namespace nexRemoteFree.Server.Migrations.Sqlite
{
    public partial class IsServerAdminproperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsServerAdmin",
                table: "nexRemoteFreeUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsServerAdmin",
                table: "nexRemoteFreeUsers");
        }
    }
}
