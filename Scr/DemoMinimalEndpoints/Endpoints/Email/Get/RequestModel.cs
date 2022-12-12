namespace DemoMinimalEndpoints.Endpoints.Email.Get;

public class RequestModel
{
	public string EmailReference { get; set; } = default!;
	public string TemplateId { get; set; } = default!;
}