using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Domain.Utilities;

namespace Domain.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class IsBase64Attribute : ValidationAttribute
{
	public override bool IsValid(object value)
	{
		return value is string base64 && FileUtil.IsBase64String(base64);
	}

	public override string FormatErrorMessage(string name)
	{
		return $"{name} is not a valid base64 string.";
	}
}