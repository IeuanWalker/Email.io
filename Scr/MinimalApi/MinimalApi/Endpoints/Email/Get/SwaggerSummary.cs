namespace FastendPointsApi.Endpoints.Email.Get;

public class SwaggerSummary : Summary<GetEmailEndpoint>
{
	public SwaggerSummary()
	{
		Summary = "Get email information";
		Description = "Get the email information for a given email reference";
		ExampleRequest = new RequestModel();
		Response<ResponseModel>(200, "ok response with body", example: new());
		Response<ErrorResponse>(400, "validation failure");
		Response(404, "account not found");
	}
}