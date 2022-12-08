using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Domain.Validation;

/// <summary>
/// Validates that the given string is a valid file name.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed partial class IsFileNameAttribute : ValidationAttribute
{
	[GeneratedRegex(@"^[\w,\s-]+\.[\w]+$", RegexOptions.Compiled)]
	private static partial Regex ValidFileNameRegex();
	public override bool IsValid(object? value)
	{
		return value is string fileName && ValidFileNameRegex().IsMatch(fileName);
	}

	public override string FormatErrorMessage(string name)
	{
		return "FileName is not valid";
	}
}