using Microsoft.EntityFrameworkCore.Migrations;

namespace App.Database.Migrations;

public partial class Initial : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "Project",
			columns: table => new
			{
				Id = table.Column<Guid>(nullable: false),
				DateModified = table.Column<DateTime>(nullable: true),
				Name = table.Column<string>(maxLength: 200, nullable: false),
				SubHeading = table.Column<string>(maxLength: 200, nullable: true),
				Description = table.Column<string>(maxLength: 500, nullable: true),
				Tags = table.Column<string>(nullable: true)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_Project", x => x.Id);
			});

		migrationBuilder.CreateTable(
			name: "Template",
			columns: table => new
			{
				Id = table.Column<Guid>(nullable: false),
				DateModified = table.Column<DateTime>(nullable: true),
				Name = table.Column<string>(maxLength: 200, nullable: false),
				ProjectId = table.Column<Guid>(nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_Template", x => x.Id);
				table.ForeignKey(
					name: "FK_Template_Project_ProjectId",
					column: x => x.ProjectId,
					principalTable: "Project",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			});

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
				IsActive = table.Column<bool>(nullable: false),
				ThumbnailImage = table.Column<string>(nullable: true),
				PreviewImage = table.Column<string>(nullable: true),
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
			name: "IX_Template_ProjectId",
			table: "Template",
			column: "ProjectId");

		migrationBuilder.CreateIndex(
			name: "IX_TemplateVersion_TemplateId",
			table: "TemplateVersion",
			column: "TemplateId");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "TemplateVersion");

		migrationBuilder.DropTable(
			name: "Template");

		migrationBuilder.DropTable(
			name: "Project");
	}
}