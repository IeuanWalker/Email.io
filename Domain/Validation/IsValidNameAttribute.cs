using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Domain.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed partial class IsValidNameAttribute : ValidationAttribute
{
	[GeneratedRegex("[±!@£$%^&*+§€#¢§¶•ªº«\\\\/<>?:;|=.]", RegexOptions.Compiled)]
	private static partial Regex InvalidCharactersRegex();
	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value is null)
		{
			return ValidationResult.Success;
		}

		if (value is not string inputString)
		{
			return ValidationResult.Success;
		}

#pragma warning disable IDE0046 // Convert to conditional expression
		if (InvalidCharactersRegex().Match(inputString).Success)
		{
			return new ValidationResult("The input string contains characters that are not allowed in this field. Blacklisted characters are ± ! @ £ $ % ^ & * + § € # ¢ § ¶ • ª º « \\\\ / < > ? : ; | = . ");
		}

		return ValidationResult.Success;
	}
}