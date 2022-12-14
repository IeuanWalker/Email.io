using System.Text.Json.Nodes;

namespace Api.Endpoints.Email.Post;

public record RequestModel
{
	// TODO: Validate no duplicate addresses
	public IEnumerable<EmailAddresses>? ToAddresses { get; init; }

	public IEnumerable<EmailAddresses>? CCAddresses { get; init; }
	public IEnumerable<EmailAddresses>? BCCAddresses { get; init; }

	/// <summary>
	/// JSON data to be used when generating the email from the template
	/// </summary>
	public JsonNode Data { get; init; } = null!;

	/// <summary>
	/// Used just for statistics, optional
	/// </summary>
	public string? Language { get; init; }

	/// <summary>
	/// Found in the admin site
	/// </summary>
	public string TemplateId { get; init; } = null!;

	public IEnumerable<AttachementsModels>? Attachments { get; init; }

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
	public string? Name { get; init; }
	public string Email { get; init; } = null!;
}

public class AttachementsModels
{
	/// <summary>
	/// Name of the file + extension, i.e. "example.txt"
	/// </summary>
	public string FileName { get; init; } = string.Empty;

	/// <summary>
	/// Base64 string of the file
	/// </summary>
	public string Content { get; init; } = string.Empty;

	/// <summary>
	/// The ContentType/ mime type of the file
	/// </summary>
	public string ContentType { get; init; } = string.Empty;
}