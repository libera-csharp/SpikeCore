using Microsoft.EntityFrameworkCore.Migrations;

namespace SpikeCore.Data.Migrations
{
    public partial class LoginMetaData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MatchType",
                table: "AspNetUserLogins",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchType",
                table: "AspNetUserLogins");
        }
    }
}
