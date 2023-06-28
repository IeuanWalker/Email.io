using System.Text.RegularExpressions;

namespace Domain.Utilities;
public static partial class EmailAddressUtil
{
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

		var emailParts = value.Split('@');

		if(emailParts.Length != 2)
		{
			return false;
		}

		if (!ValidateLocalPart(emailParts[0]))
		{
			return false;
		}

		if (!ValidateDomain(emailParts[1]))
		{
			return false;
		}

		return true;
	}

	[GeneratedRegex(@"^[A-Za-z0-9!#$%&'*+\-/=?^_`{|}~.]+$", RegexOptions.Compiled)]
	private static partial Regex LocalPartAllowedCharactersRegex();
	[GeneratedRegex(@"\.{2,}", RegexOptions.Compiled)]
	private static partial Regex LocalPartMultipleFullStopsRegex();
	static bool ValidateLocalPart(string localPart)
	{
		if (localPart.Length > 64)
		{
			return false;
		}

		if (!LocalPartAllowedCharactersRegex().Match(localPart).Success)
		{
			return false;
		}

		if(localPart.StartsWith(".") || localPart.EndsWith(".") || LocalPartMultipleFullStopsRegex().Match(localPart).Success)
		{
			return false;
		}

		return true;
	}

	[GeneratedRegex(@"^(?!:\/\/)([a-zA-Z0-9][a-zA-Z0-9-]{0,62}\.)+[a-zA-Z0-9][a-zA-Z0-9-]{0,62}$", RegexOptions.Compiled)]
	private static partial Regex DomainPartStandardDomainRegex();
	[GeneratedRegex(@"^\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\]$", RegexOptions.Compiled)]
	private static partial Regex DomainPartIPv4Regex();
	[GeneratedRegex(@"(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))", RegexOptions.Compiled)]
	private static partial Regex DomainPartIPv6Regex();
	static bool ValidateDomain(string domainPart)
	{
		if (domainPart.Length > 255)
		{
			return false;
		}

		if (domainPart.StartsWith("-") || domainPart.EndsWith("-"))
		{
			return false;
		}

		if (DomainPartStandardDomainRegex().Match(domainPart).Success)
		{
			return true;
		}

		if (DomainPartIPv4Regex().Match(domainPart).Success)
		{
			return true;
		}

		if (DomainPartIPv6Regex().Match(domainPart).Success)
		{
			return true;
		}

		return false;
	}

	[GeneratedRegex("[±!@£$%^&*+§€#¢§¶•ªº«\\\\/<>?:;|=.]", RegexOptions.Compiled)]
	private static partial Regex InvalidCharactersRegex();
	public static bool IsValidName(string value)
	{
		return !InvalidCharactersRegex().Match(value).Success;
	}
}
