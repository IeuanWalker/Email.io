using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations;

/// <inheritdoc />
public partial class CreatedProjectUsersTbl : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProjectUsers",
            columns: table => new
            {
                ProjectId = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<int>(type: "int", nullable: false),
                Role = table.Column<string>(type: "nvarchar(8)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectUsers", x => new { x.ProjectId, x.UserId });
                table.ForeignKey(
                    name: "FK_ProjectUsers_Project_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Project",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProjectUsers_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProjectUsers_UserId",
            table: "ProjectUsers",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ProjectUsers");
    }
}