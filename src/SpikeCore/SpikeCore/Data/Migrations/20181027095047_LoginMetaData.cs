using Microsoft.EntityFrameworkCore.Migrations;

namespace SpikeCore.Data.Migrations
{
    public partial class LoginMetaData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MetaData",
                table: "AspNetUserLogins",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MetaData",
                table: "AspNetUserLogins");
        }
    }
}
