using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

[Table("EmailAttachment")]
public class EmailAttachmentTbl
{
	[Key]
	public int Id { get; set; }
	public string FileName { get; set; } = default!;
	public string ContentType { get; set; } = default!;
	public string Content { get; set; } = default!;
}
