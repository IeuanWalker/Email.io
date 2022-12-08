using System.ComponentModel.DataAnnotations;
using MimeKit;

namespace Domain.Validation;

/// <summary>
/// Validates that the given string is a valid content type.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class IsContentTypeAttribute : ValidationAttribute
{
	const string contentTypeRequiredErrorMessage = "ContentType is required.";
	const string invalidContentTypeErrorMessage = "ContentType is invalid.";

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value is not string contentType || string.IsNullOrWhiteSpace(contentType))
		{
			return new ValidationResult(contentTypeRequiredErrorMessage);
		}

		try
		{
			if (ContentType.TryParse(contentType, out _))
			{
				return ValidationResult.Success;
			}
		}
		catch (Exception)
		{
			return new ValidationResult(invalidContentTypeErrorMessage);
		}

		return new ValidationResult(invalidContentTypeErrorMessage);
	}
}