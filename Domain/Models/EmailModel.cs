using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Domain.Validation;

namespace Domain.Models;

public class EmailModel
{
	// TODO: Validate no duplicate addresses
	public IEnumerable<EmailAddresses>? ToAddresses { get; set; }
	public IEnumerable<EmailAddresses>? CCAddresses { get; set; }
	public IEnumerable<EmailAddresses>? BCCAddresses { get; set; }

	/// <summary>
	/// Data to be used when generating the email from the template
	/// </summary>
	[Required]
	public JsonNode Data { get; set; } = null!;

	/// <summary>
	/// Used just for statistics, optional
	/// </summary>
	[MaxLength(10)]
	public string? Language { get; set; }

	/// <summary>
	/// Found in the admin site
	/// </summary>
	[Required]
	[MinLength(30)]
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
	/// <summary>
	/// Name of the recpient, optional
	/// </summary>
	[MaxLength(200)]
	[IsValidName]
	public string? Name { get; set; }

	[Required]
	[MaxLength(200)]
	[IsEmail]
	public string Email { get; set; } = null!;
}

public class AttachementsModels
{
	/// <summary>
	/// Name of the file + extension, i.e. "example.txt"
	/// </summary>
	[Required]
	[MinLength(3)]
	[IsFileName]
	public string FileName { get; set; } = string.Empty;
	/// <summary>
	/// Base64 string of the file
	/// </summary>
	[Required]
	[MinLength(1)]
	[IsBase64]
	public string Content { get; set; } = string.Empty;
	/// <summary>
	/// The ContentType/ mime type of the file
	/// </summary>
	[Required]
	[MinLength(1)]
	[IsContentType]
	public string ContentType { get; set; } = string.Empty;
}