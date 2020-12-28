using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace App.Database.Migrations
{
    public partial class FixRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateTbl_ProjectTbl_ProjectTblId",
                table: "TemplateTbl");

            migrationBuilder.DropIndex(
                name: "IX_TemplateTbl_ProjectTblId",
                table: "TemplateTbl");

            migrationBuilder.DropColumn(
                name: "ProjectTblId",
                table: "TemplateTbl");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTbl_ProjectId",
                table: "TemplateTbl",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateTbl_ProjectTbl_ProjectId",
                table: "TemplateTbl",
                column: "ProjectId",
                principalTable: "ProjectTbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateTbl_ProjectTbl_ProjectId",
                table: "TemplateTbl");

            migrationBuilder.DropIndex(
                name: "IX_TemplateTbl_ProjectId",
                table: "TemplateTbl");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectTblId",
                table: "TemplateTbl",
                type: "uniqueidentifier",
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
    }
}