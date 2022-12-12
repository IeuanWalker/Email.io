using System.Text.RegularExpressions;

namespace Domain.Utilities;
public static partial class EmailAddressUtil
{
	/// <summary>
	/// A combination of 3 different regex to validate emails based on the RFC 5322 spec
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])|(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))|(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$", RegexOptions.Compiled)]
	private static partial Regex EmailRegex();

	/// <summary>
	/// Matches if the start or end character is a full stop
	/// Matches if there are multiple @ symbols
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex(@"^\.|\.$|@.*@", RegexOptions.Compiled)]
	private static partial Regex AdditionalValidationRegex();

	/// <summary>
	/// Attempted RFC 5322 complient email validation
	/// </summary>
	/// <param name="value">The email address to validate.</param>
	/// <returns>True if the email address is valid, otherwise false.</returns>
	public static bool IsValidEmailAddress(string value)
	{
		// Check if the value is null or empty
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}

		// Check general email regex
		if (!EmailRegex().IsMatch(value))
		{
			return false;
		}

		// Starts or ends in a fullstop
		if (AdditionalValidationRegex().IsMatch(value))
		{
			return false;
		}

		// TODO: Benchmark if this is faster than the regex
		//if (value.StartsWith('.') || value.EndsWith('.') || value.Split('@').Length > 2)
		//{
		//	return false;
		//}

		return true;
	}

	[GeneratedRegex("[±!@£$%^&*+§€#¢§¶•ªº«\\\\/<>?:;|=.]", RegexOptions.Compiled)]
	private static partial Regex InvalidCharactersRegex();
	public static bool IsValidName(string value)
	{
		return !InvalidCharactersRegex().Match(value).Success;
	}
}
