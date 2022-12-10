namespace MinimalApi.Endpoints.Email.Post;

public class SwaggerSummary : Summary<PostEmailEndpoint>
{
	public SwaggerSummary()
	{
		Summary = "Send an email";
		Description = "Send an email using a pre configured template";
		ExampleRequest = new RequestModel();
		Response<ResponseModel>(200, "ok response with body", example: new());
		Response<ErrorResponse>(400, "validation failure");
		Response(404, "account not found");
	}
}