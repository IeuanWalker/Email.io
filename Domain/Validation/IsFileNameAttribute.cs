using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Domain.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed partial class IsFileNameAttribute : ValidationAttribute
{
	[GeneratedRegex(@"[^-_.A-Za-z0-9]", RegexOptions.Compiled)]
	private static partial Regex ValidFileName();
	public override bool IsValid(object value)
	{
		return value is string fileName && !ValidFileName().IsMatch(fileName);
	}

	public override string FormatErrorMessage(string name)
	{
		return "FileName contains invalid characters. Valid characters are : A-Z a-z 0-9 - _ .";
	}
}