using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

[Table("Project")]
public class ProjectTbl : BaseEntityModifiedDate
{
	[Key]
	public Guid Id { get; set; }

	[Required]
	[MaxLength(200)]
	public string Name { get; set; } = string.Empty;

	[DisplayName("Sub heading")]
	[MaxLength(200)]
	public string? SubHeading { get; set; }

	[MaxLength(500)]
	public string? Description { get; set; }

	public string? Tags { get; set; }
	public ICollection<TemplateTbl>? Templates { get; set; }
}