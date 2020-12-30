using Microsoft.EntityFrameworkCore.Migrations;

namespace App.Database.Migrations
{
    public partial class ProjectRenameTitleToName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("Title", "Project", "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.RenameColumn("Name", "Project", "Title");
        }
    }
}
