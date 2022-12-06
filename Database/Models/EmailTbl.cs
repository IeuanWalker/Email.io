using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

[Table("Email")]
public class EmailTbl
{
	[Key]
	public int Id { get; set; }

	public int TemplateId { get; set; }
	public string Data { get; set; } = string.Empty;



	[ForeignKey("ToAddressesEmailId")]
	public ICollection<EmailAddressTbl> ToAddresses { get; set; } = new List<EmailAddressTbl>();

	[ForeignKey("CCAddressesEmailId")]
	public ICollection<EmailAddressTbl>? CCAddresses { get; set; }

	[ForeignKey("BCCAddressesEmailId")]
	public ICollection<EmailAddressTbl>? BCCAddresses { get; set; }

	public string Subject { get; set; } = string.Empty;
	public string HtmlContent { get; set; } = string.Empty;
	public string PlainTextContent { get; set; } = string.Empty;

	[MaxLength(5)]
	public string Language { get; set; } = "en-GB";

	[ForeignKey("AttachementsId")]
	public ICollection<EmailAttachmentTbl>? Attachements { get; set; }
	public int AttachementCount { get; set; }

	public string? HangfireId { get; set; }
	public DateTime? Sent { get; set; }

	// Relationship
	public int ProjectId { get; set; }

	public ProjectTbl? Project { get; set; }
}