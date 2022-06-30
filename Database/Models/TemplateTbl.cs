﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

[Table("Template")]
public class TemplateTbl : BaseEntityModifiedDate
{
	[Key]
	public Guid Id { get; set; }

	[Required]
	[MaxLength(200)]
	public string Name { get; set; } = string.Empty;

	public Guid ProjectId { get; set; }
	public ProjectTbl? Project { get; set; }
	public ICollection<TemplateVersionTbl>? Versions { get; set; }
}