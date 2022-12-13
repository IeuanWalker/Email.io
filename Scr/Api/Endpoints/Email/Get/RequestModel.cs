namespace Api.Endpoints.Email.Get;

public record RequestModel
{
	public string EmailReference { get; set; } = default!;
	public string TemplateId { get; set; } = default!;
}