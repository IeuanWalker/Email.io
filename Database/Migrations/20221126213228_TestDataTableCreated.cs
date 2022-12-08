using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations;

/// <inheritdoc />
public partial class TestDataTableCreated : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "TestData",
			table: "TemplateVersion");

		migrationBuilder.CreateTable(
			name: "TemplateTestData",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("SqlServer:Identity", "1, 1"),
				Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
				Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
				IsDefault = table.Column<bool>(type: "bit", nullable: false),
				TemplateVersionId = table.Column<int>(type: "int", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_TemplateTestData", x => x.Id);
				table.ForeignKey(
					name: "FK_TemplateTestData_TemplateVersion_TemplateVersionId",
					column: x => x.TemplateVersionId,
					principalTable: "TemplateVersion",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "IX_TemplateTestData_TemplateVersionId",
			table: "TemplateTestData",
			column: "TemplateVersionId");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "TemplateTestData");

		migrationBuilder.AddColumn<string>(
			name: "TestData",
			table: "TemplateVersion",
			type: "nvarchar(max)",
			nullable: true);
	}
}