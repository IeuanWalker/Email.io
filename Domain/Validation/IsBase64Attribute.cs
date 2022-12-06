using System.ComponentModel.DataAnnotations;
using Domain.Utilities;

namespace Domain.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class IsBase64Attribute : ValidationAttribute
{
	const string contentNotValidErrorMessage = "Content is not a valid base 64 string.";
	const string contentRequiredErrorMessage = "Content is required.";

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value is not string base64 || string.IsNullOrWhiteSpace(base64))
		{
			return new ValidationResult(contentRequiredErrorMessage);
		}

		if (FileUtil.IsBase64String(base64))
		{
			return ValidationResult.Success;
		}

		return new ValidationResult(contentNotValidErrorMessage);
	}
}