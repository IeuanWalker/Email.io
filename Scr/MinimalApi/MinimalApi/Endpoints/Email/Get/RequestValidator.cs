namespace MinimalApi.Endpoints.Email.Get;

public class RequestModelValidator : Validator<RequestModel>
{
	public RequestModelValidator()
	{
		RuleFor(x => x.EmailReference)
			.NotEmpty()
			.MinimumLength(30);

		RuleFor(x => x.TemplateId)
			.NotEmpty()
			.MinimumLength(30);
	}
}