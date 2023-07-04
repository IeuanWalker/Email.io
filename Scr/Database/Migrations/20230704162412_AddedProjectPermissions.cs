using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations;

/// <inheritdoc />
public partial class AddedProjectPermissions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "CanCreateApiKey",
            table: "ProjectUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "CanCreateTemplate",
            table: "ProjectUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "CanCreateTemplateVersion",
            table: "ProjectUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "CanDeleteApiKey",
            table: "ProjectUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "CanDeleteTemplate",
            table: "ProjectUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "CanDeleteTemplateVersion",
            table: "ProjectUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "CanEditProject",
            table: "ProjectUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "CanEditTemplate",
            table: "ProjectUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "CanEditTemplateVersion",
            table: "ProjectUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "CanResetApiKey",
            table: "ProjectUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "CanViewActivityLog",
            table: "ProjectUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "CanViewApiKeys",
            table: "ProjectUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "CanViewSentEmails",
            table: "ProjectUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CanCreateApiKey",
            table: "ProjectUsers");

        migrationBuilder.DropColumn(
            name: "CanCreateTemplate",
            table: "ProjectUsers");

        migrationBuilder.DropColumn(
            name: "CanCreateTemplateVersion",
            table: "ProjectUsers");

        migrationBuilder.DropColumn(
            name: "CanDeleteApiKey",
            table: "ProjectUsers");

        migrationBuilder.DropColumn(
            name: "CanDeleteTemplate",
            table: "ProjectUsers");

        migrationBuilder.DropColumn(
            name: "CanDeleteTemplateVersion",
            table: "ProjectUsers");

        migrationBuilder.DropColumn(
            name: "CanEditProject",
            table: "ProjectUsers");

        migrationBuilder.DropColumn(
            name: "CanEditTemplate",
            table: "ProjectUsers");

        migrationBuilder.DropColumn(
            name: "CanEditTemplateVersion",
            table: "ProjectUsers");

        migrationBuilder.DropColumn(
            name: "CanResetApiKey",
            table: "ProjectUsers");

        migrationBuilder.DropColumn(
            name: "CanViewActivityLog",
            table: "ProjectUsers");

        migrationBuilder.DropColumn(
            name: "CanViewApiKeys",
            table: "ProjectUsers");

        migrationBuilder.DropColumn(
            name: "CanViewSentEmails",
            table: "ProjectUsers");
    }
}