using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace App.Database.Migrations
{
    public partial class ProjectAndTemplateRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "TemplateTbl",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectTblId",
                table: "TemplateTbl",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTbl_ProjectTblId",
                table: "TemplateTbl",
                column: "ProjectTblId");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateTbl_ProjectTbl_ProjectTblId",
                table: "TemplateTbl",
                column: "ProjectTblId",
                principalTable: "ProjectTbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateTbl_ProjectTbl_ProjectTblId",
                table: "TemplateTbl");

            migrationBuilder.DropIndex(
                name: "IX_TemplateTbl_ProjectTblId",
                table: "TemplateTbl");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "TemplateTbl");

            migrationBuilder.DropColumn(
                name: "ProjectTblId",
                table: "TemplateTbl");
        }
    }
}