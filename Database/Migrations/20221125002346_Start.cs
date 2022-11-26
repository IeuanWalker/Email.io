using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
	public partial class Start : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Project",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
					SubHeading = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
					Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
					Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
					DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Project", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Email",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					TemplateId = table.Column<int>(type: "int", nullable: false),
					Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
					HtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
					PlainTextContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Language = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
					HangfireId = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Sent = table.Column<DateTime>(type: "datetime2", nullable: true),
					ProjectId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Email", x => x.Id);
					table.ForeignKey(
						name: "FK_Email_Project_ProjectId",
						column: x => x.ProjectId,
						principalTable: "Project",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Template",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
					ProjectId = table.Column<int>(type: "int", nullable: false),
					DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
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
				name: "EmailAddress",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
					BCCAddressesEmailId = table.Column<int>(type: "int", nullable: true),
					CCAddressesEmailId = table.Column<int>(type: "int", nullable: true),
					ToAddressesEmailId = table.Column<int>(type: "int", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_EmailAddress", x => x.Id);
					table.ForeignKey(
						name: "FK_EmailAddress_Email_BCCAddressesEmailId",
						column: x => x.BCCAddressesEmailId,
						principalTable: "Email",
						principalColumn: "Id");
					table.ForeignKey(
						name: "FK_EmailAddress_Email_CCAddressesEmailId",
						column: x => x.CCAddressesEmailId,
						principalTable: "Email",
						principalColumn: "Id");
					table.ForeignKey(
						name: "FK_EmailAddress_Email_ToAddressesEmailId",
						column: x => x.ToAddressesEmailId,
						principalTable: "Email",
						principalColumn: "Id");
				});

			migrationBuilder.CreateTable(
				name: "TemplateVersion",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
					Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
					TestData = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Html = table.Column<string>(type: "nvarchar(max)", nullable: true),
					PlainText = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Categories = table.Column<string>(type: "nvarchar(max)", nullable: true),
					IsActive = table.Column<bool>(type: "bit", nullable: false),
					ThumbnailImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
					PreviewImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
					TemplateId = table.Column<int>(type: "int", nullable: false),
					DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
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
				name: "IX_Email_ProjectId",
				table: "Email",
				column: "ProjectId");

			migrationBuilder.CreateIndex(
				name: "IX_EmailAddress_BCCAddressesEmailId",
				table: "EmailAddress",
				column: "BCCAddressesEmailId");

			migrationBuilder.CreateIndex(
				name: "IX_EmailAddress_CCAddressesEmailId",
				table: "EmailAddress",
				column: "CCAddressesEmailId");

			migrationBuilder.CreateIndex(
				name: "IX_EmailAddress_ToAddressesEmailId",
				table: "EmailAddress",
				column: "ToAddressesEmailId");

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
				name: "EmailAddress");

			migrationBuilder.DropTable(
				name: "TemplateVersion");

			migrationBuilder.DropTable(
				name: "Email");

			migrationBuilder.DropTable(
				name: "Template");

			migrationBuilder.DropTable(
				name: "Project");
		}
	}
}