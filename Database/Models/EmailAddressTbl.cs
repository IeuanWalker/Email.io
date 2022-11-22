using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

[Table("EmailAddress")]
public class EmailAddressTbl
{
	[Key]
	public int Id { get; set; }

	public string? Name { get; set; }
	public string Email { get; set; } = string.Empty;
}