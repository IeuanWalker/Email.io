using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations;

/// <inheritdoc />
public partial class ChangeColumnNameFromSavedFileToContent : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.RenameColumn(
			name: "SavedFile",
			table: "EmailAttachment",
			newName: "Content");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.RenameColumn(
			name: "Content",
			table: "EmailAttachment",
			newName: "SavedFile");
	}
}