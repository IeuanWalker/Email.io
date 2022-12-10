using System.Net;

namespace Api.Endpoints.Email.Post;

public class SwaggerSummary : Summary<PostEmailEndpoint>
{
	public SwaggerSummary()
	{
		Summary = "Send an email";
		Description = "Send an email using a pre configured template";
		ExampleRequest = new RequestModel
		{
			ToAddresses = new List<EmailAddresses>
			{
				new EmailAddresses
				{
					Name = "Example name",
					Email = "example@email.com"
				}
			},
			TemplateId = "YOUR TEMPLATE ID",
		};
		Response<ResponseModel>((int)HttpStatusCode.OK, "Email received", example: new() { 
			Reference  = "abc123" 
		});
		Response<ErrorResponse>((int)HttpStatusCode.BadRequest, "Failed validation", example: new()
		{
			StatusCode = (int)HttpStatusCode.BadRequest,
			Message = "One or more errors occured!",
			Errors = new Dictionary<string, List<string>>
			{
				{"Property with error", new List<string>{"error1", "error2", "error3"}},
			}
		});
		Response((int)HttpStatusCode.Unauthorized, "Invalid or missing API key");
		Response<InternalErrorResponse>((int)HttpStatusCode.InternalServerError, "Unhandled internal error", "application/problem+json", new()
		{
			Status = "InternalServerError",
			Code = 500,
			Reason = "Unknown",
			Note = "See application log for stack trace."
		});
	}
}