using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace App.Database.Migrations
{
    public partial class TemplateVersionTblCreated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TemplateVersion",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateModified = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Subject = table.Column<string>(maxLength: 200, nullable: false),
                    TestData = table.Column<string>(nullable: true),
                    Html = table.Column<string>(nullable: true),
                    Categories = table.Column<string>(nullable: true),
                    TemplateId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateVersion_Template_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Template",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateVersion_TemplateId",
                table: "TemplateVersion",
                column: "TemplateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateVersion");
        }
    }
}
