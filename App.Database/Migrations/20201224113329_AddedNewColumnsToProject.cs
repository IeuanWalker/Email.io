using Microsoft.EntityFrameworkCore.Migrations;

namespace App.Database.Migrations
{
    public partial class AddedNewColumnsToProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubHeading",
                table: "ProjectTbl",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "ProjectTbl",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubHeading",
                table: "ProjectTbl");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "ProjectTbl");
        }
    }
}