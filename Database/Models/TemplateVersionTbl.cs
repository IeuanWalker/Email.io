using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

[Table("TemplateVersion")]
public class TemplateVersionTbl : BaseEntityModifiedDate
{
	[Key]
	public int Id { get; set; }

	[Required]
	[MaxLength(200)]
	public string Name { get; set; } = default!;

	[Required]
	[MaxLength(200)]
	public string Subject { get; set; } = default!;

	public string? Html { get; set; }
	public string? PlainText { get; set; }
	public string? Categories { get; set; }
	public bool IsActive { get; set; }
	public string? ThumbnailImage { get; set; }
	public string? PreviewImage { get; set; }
	public ICollection<TemplateTestDataTbl> TestData { get; set; } = new List<TemplateTestDataTbl>();
	public int TemplateId { get; set; }
	public TemplateTbl Template { get; set; } = null!;
}