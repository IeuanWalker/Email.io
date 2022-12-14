namespace Api.Endpoints.Email.Get;

public record RequestModel
{
	public string EmailReference { get; init; } = default!;
	public string TemplateId { get; init; } = default!;
}