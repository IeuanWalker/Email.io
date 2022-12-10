using FastEndpoints;

namespace MinimalApi.Endpoints.Email.Get;

public class GetEmailEndpoint : Endpoint<RequestModel, ResponseModel>
{
	public override void Configure()
	{
		Get("email");
		Version(1);
	}
	
	public override async Task HandleAsync(RequestModel request, CancellationToken ct)
	{
		await SendAsync(new ResponseModel
		{
			Reference = request.EmailReference
		}, cancellation: ct);
	}
}