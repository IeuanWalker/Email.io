using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

[Table("User")]
public class UserTbl
{
	[Key]
	public int Id { get; set; }

	public string Sub { get; set; } = null!;

	[Column(TypeName = "nvarchar(8)")]
	public UserRoles Role { get; set; } = UserRoles.Standard;

	[MaxLength(400)]
	public string Email { get; set; } = null!;

	public string? GivenName { get; set; }
	public string? FamilyName { get; set; }
	public string DisplayName { get; set; } = null!;

	[MaxLength(2)]
	public string Initials { get; set; } = null!;

	// TODO: Last active
}

public enum UserRoles
{
	Standard,
	Admin
}