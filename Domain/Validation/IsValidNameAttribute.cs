using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Domain.Utilities;

namespace Domain.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed partial class IsValidNameAttribute : ValidationAttribute
{
	[GeneratedRegex("([^±!@£$%^&*_+§¡€#¢§¶•ªº«\\/<>?:;|=.,])", RegexOptions.Compiled)]
	private static partial Regex ValidName();
	public override bool IsValid(object value)
	{
		if (value is not string fileName)
		{
			return false;
		}

		var test = ValidName().IsMatch(fileName);
		var test2 = ValidName().Matches(fileName);

		return !test;
	}

	public override string FormatErrorMessage(string name)
	{
		return $"{name} contains invalid characters. These characters are not allowed: ^ ± ! @ £ $ % ^ & * _ + § ¡ € # ¢ § ¶ • ª º « \\ / < > ? : ; | = . ,";
	}
}