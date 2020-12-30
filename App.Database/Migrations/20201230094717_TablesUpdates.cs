using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace App.Database.Migrations
{
    public partial class TablesUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateTbl_ProjectTbl_ProjectId",
                table: "TemplateTbl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TemplateTbl",
                table: "TemplateTbl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectTbl",
                table: "ProjectTbl");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "TemplateTbl");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "TemplateTbl");

            migrationBuilder.DropColumn(
                name: "Template",
                table: "TemplateTbl");

            migrationBuilder.DropColumn(
                name: "TestData",
                table: "TemplateTbl");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "ProjectTbl");

            migrationBuilder.RenameTable(
                name: "TemplateTbl",
                newName: "Template");

            migrationBuilder.RenameTable(
                name: "ProjectTbl",
                newName: "Project");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateTbl_ProjectId",
                table: "Template",
                newName: "IX_Template_ProjectId");

            migrationBuilder.AlterColumn<string>(
                name: "SubHeading",
                table: "Project",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Template",
                table: "Template",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Project",
                table: "Project",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Template_Project_ProjectId",
                table: "Template",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Template_Project_ProjectId",
                table: "Template");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Template",
                table: "Template");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Project",
                table: "Project");

            migrationBuilder.RenameTable(
                name: "Template",
                newName: "TemplateTbl");

            migrationBuilder.RenameTable(
                name: "Project",
                newName: "ProjectTbl");

            migrationBuilder.RenameIndex(
                name: "IX_Template_ProjectId",
                table: "TemplateTbl",
                newName: "IX_TemplateTbl_ProjectId");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "TemplateTbl",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TemplateTbl",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Template",
                table: "TemplateTbl",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestData",
                table: "TemplateTbl",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubHeading",
                table: "ProjectTbl",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "ProjectTbl",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TemplateTbl",
                table: "TemplateTbl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectTbl",
                table: "ProjectTbl",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateTbl_ProjectTbl_ProjectId",
                table: "TemplateTbl",
                column: "ProjectId",
                principalTable: "ProjectTbl",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
