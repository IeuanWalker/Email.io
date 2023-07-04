using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Models;
[Table("ProjectUsers")]
[PrimaryKey(nameof(ProjectId), nameof(UserId))]
public class ProjectUserTbl
{
	public int ProjectId { get; set; }
	public ProjectTbl Project { get; set; } = default!;
	public int UserId { get; set; }
	public UserTbl User { get; set; } = default!;
	[Column(TypeName = "nvarchar(8)")]
	public ProjectUserRoles Role { get; set; }

	// Permissions
	public bool CanCreateTemplate { get; set; }
	public bool CanEditTemplate { get; set; }
	public bool CanDeleteTemplate { get; set; }
	public bool CanCreateTemplateVersion { get; set; }
	public bool CanEditTemplateVersion { get; set; }
	public bool CanDeleteTemplateVersion { get; set; }
	public bool CanCreateApiKey { get; set; }
	public bool CanResetApiKey { get; set; }
	public bool CanDeleteApiKey { get; set; }
	public bool CanViewApiKeys { get; set; }
	public bool CanViewSentEmails { get; set; }
	public bool CanViewActivityLog { get; set; }
	public bool CanEditProject { get; set; }
}


public enum ProjectUserRoles
{
	Standard,
	Owner
}