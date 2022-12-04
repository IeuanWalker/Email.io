using System.ComponentModel.DataAnnotations;
using MimeKit;

namespace Domain.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class IsContentTypeAttribute : ValidationAttribute
{
	public override bool IsValid(object value)
	{
		return value is string contentType && ContentType.TryParse(contentType, out _);
	}

	public override string FormatErrorMessage(string name)
	{
		return $"{name} is not a valid content type. Use MimeKit to validate - https://github.com/jstedfast/MimeKit";
	}
}