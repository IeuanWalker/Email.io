using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;

namespace Domain.Models;

public class EmailModel
{
	// TODO: Validate no duplicate addresses
	public IEnumerable<EmailAddresses>? ToAddresses { get; set; }
	public IEnumerable<EmailAddresses>? CCAddresses { get; set; }
	public IEnumerable<EmailAddresses>? BCCAddresses { get; set; }

	[Required]
	public JsonNode Data { get; set; } = null!;

	[MaxLength(5)]
	public string Language { get; set; } = "en-GB";

	[Required]
	public string TemplateId { get; set; } = null!;

	public IEnumerable<AttachementsModels>? Attachments { get; set; }

	// TODO: Support specific time to send email
	// TODO: Support canceling scheduled emails
	// TODO: Use unix timestamps
	//public int? SendAt { get; set; }
	// TODO: Support categories for stats
}

public class EmailAddresses
{
	public string? Name { get; set; }

	[EmailAddress]
	public string Email { get; set; } = null!;
}

public class AttachementsModels
{
	[Required]
	[MinLength(1)]
	public string Content { get; set; } = string.Empty;
	[Required]
	[MinLength(3)]
	// [^-_.A-Za-z0-9]
	public string FileName { get; set; } = string.Empty;
	[Required]
	[MinLength(1)]
	public string Type { get; set; } = string.Empty;
}