using Microsoft.EntityFrameworkCore.Migrations;

namespace App.Database.Migrations
{
    public partial class ImageColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviewImage",
                table: "TemplateVersion",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailImage",
                table: "TemplateVersion",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewImage",
                table: "TemplateVersion");

            migrationBuilder.DropColumn(
                name: "ThumbnailImage",
                table: "TemplateVersion");
        }
    }
}
