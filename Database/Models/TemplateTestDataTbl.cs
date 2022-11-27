using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

[Table("TemplateTestData")]
public class TemplateTestDataTbl
{
	[Key]
	public int Id { get; set; }
	[MaxLength(200)]
	public string Name { get; set; } = default!;
	public string Data { get; set; } = default!;
	public bool IsDefault { get; set; }

	public int TemplateVersionId { get; set; }
	public TemplateVersionTbl? TemplateVersion { get; set; }
}
