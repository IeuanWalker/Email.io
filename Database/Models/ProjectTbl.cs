using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

[Table("Project")]
public class ProjectTbl : BaseEntityModifiedDate
{
	[Key]
	public int Id { get; set; }

	[Required]
	[MaxLength(200)]
	public string Name { get; set; } = default!;

	[DisplayName("Sub heading")]
	[MaxLength(200)]
	public string? SubHeading { get; set; }

	[MaxLength(500)]
	public string? Description { get; set; }

	public string? Tags { get; set; }
	public string ApiKey { get; set; } = default!;
	public ICollection<TemplateTbl>? Templates { get; set; }
}