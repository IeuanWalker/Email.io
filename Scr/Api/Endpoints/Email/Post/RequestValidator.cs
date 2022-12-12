using Domain.Utilities;

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
			.MaximumLength(5);

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
				.MaximumLength(200)
				.Must(EmailAddressUtil.IsValidName)
				.WithMessage("Not a valid name");

			RuleFor(x => x.Email)
				.NotEmpty()
				.MaximumLength(320)
				.Must(EmailAddressUtil.IsValidEmailAddress) // Max characters before the @ is 64 characters, and the maximum length of the domain part is 255 characters
				.WithMessage("Not a valid email address");
		}
	}

	public class AttachementsValidator : Validator<AttachementsModels>
	{
		public AttachementsValidator()
		{
			RuleFor(x => x.FileName)
				.NotEmpty()
				.MinimumLength(3)
				.Must(FileUtil.IsFileName)
				.WithMessage("FileName not a valid.");

			RuleFor(x => x.Content)
				.NotEmpty()
				.MinimumLength(1)
				.Must(FileUtil.IsBase64String)
				.WithMessage("Content not a valid base 64 string.");

			RuleFor(x => x.ContentType)
				.NotEmpty()
				.MinimumLength(1)
				.Must(FileUtil.IsContentType)
				.WithMessage("ContentType is not a valid ContentType");
		}
	}
}