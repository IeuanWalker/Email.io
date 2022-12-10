namespace Api.Endpoints.Email.Post;

public class RequestModelValidator : Validator<RequestModel>
{
	public RequestModelValidator()
	{
		RuleForEach(x => x.ToAddresses).SetValidator(new EmailAddressesValidator());
		RuleForEach(x => x.CCAddresses).SetValidator(new EmailAddressesValidator());
		RuleForEach(x => x.BCCAddresses).SetValidator(new EmailAddressesValidator());

		RuleFor(x => x.Data)
			.NotEmpty();

		RuleFor(x => x.Language)
			.NotEmpty();

		RuleFor(x => x.TemplateId)
			.NotEmpty()
			.MinimumLength(30);

		RuleForEach(x => x.Attachments).SetValidator(new AttachementsValidator());
	}

	public class EmailAddressesValidator : Validator<EmailAddresses>
	{
		public EmailAddressesValidator()
		{
			RuleFor(x => x.Name)
				.MaximumLength(200);

			RuleFor(x => x.Name)
				.NotEmpty()
				.MaximumLength(320); // Max characters before the @ is 64 characters, and the maximum length of the domain part is 255 characters
		}
	}

	public class AttachementsValidator : Validator<AttachementsModels>
	{
		public AttachementsValidator()
		{
			RuleFor(x => x.FileName)
				.NotEmpty()
				.MinimumLength(3);

			RuleFor(x => x.Content)
				.NotEmpty()
				.MinimumLength(1);

			RuleFor(x => x.ContentType)
				.NotEmpty()
				.MinimumLength(1);
		}
	}
}