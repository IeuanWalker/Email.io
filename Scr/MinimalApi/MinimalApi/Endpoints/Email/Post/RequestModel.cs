using System.Text.Json.Nodes;
using Domain.Validation;

namespace MinimalApi.Endpoints.Email.Post;

public class RequestModel
{
	// TODO: Validate no duplicate addresses
	public IEnumerable<EmailAddresses>? ToAddresses { get; set; }

	public IEnumerable<EmailAddresses>? CCAddresses { get; set; }
	public IEnumerable<EmailAddresses>? BCCAddresses { get; set; }

	/// <summary>
	/// Data to be used when generating the email from the template
	/// </summary>
	public JsonNode Data { get; set; } = null!;

	/// <summary>
	/// Used just for statistics, optional
	/// </summary>
	public string? Language { get; set; }

	/// <summary>
	/// Found in the admin site
	/// </summary>
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
	[IsValidName]
	public string? Name { get; set; }

	[IsEmail]
	public string Email { get; set; } = null!;
}

public class AttachementsModels
{
	/// <summary>
	/// Name of the file + extension, i.e. "example.txt"
	/// </summary>
	[IsFileName]
	public string FileName { get; set; } = string.Empty;

	/// <summary>
	/// Base64 string of the file
	/// </summary>
	[IsBase64]
	public string Content { get; set; } = string.Empty;

	/// <summary>
	/// The ContentType/ mime type of the file
	/// </summary>
	[IsContentType]
	public string ContentType { get; set; } = string.Empty;
}
