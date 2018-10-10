using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SpikeCore.Data.Migrations
{
    public partial class createfactoids : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Factoids",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Type = table.Column<string>(maxLength: 255, nullable: false),
                    Description = table.Column<string>(maxLength: 4000, nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factoids", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Factoids_Name",
                table: "Factoids",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Factoids");
        }
    }
}
