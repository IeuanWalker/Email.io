using Microsoft.EntityFrameworkCore.Migrations;

namespace App.Database.Migrations
{
    public partial class AddedColumnTemplateVersionIsActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TemplateVersion",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TemplateVersion");
        }
    }
}
