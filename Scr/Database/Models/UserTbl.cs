using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Models;

[Table("User")]
[Index(nameof(Sub), nameof(Iss), IsUnique = true)]
public class UserTbl
{
	[Key]
	public int Id { get; set; }
	/// <summary>
	/// User's unique identifier from OCID provider
	/// </summary>
	public string Sub { get; set; } = null!;
	/// <summary>
	/// Issuer of the user's unique identifier
	/// </summary>
	public string Iss { get; set; } = null!;

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