using System.Text.RegularExpressions;
using MimeKit;

namespace Domain.Utilities;

public static partial class FileUtil
{
	/// <summary>
	/// Validates a Base64 string.
	/// </summary>
	/// <param name="value"></param>
	public static bool IsBase64String(string value)
	{
		if (value is not string base64 || string.IsNullOrWhiteSpace(base64))
		{
			return false;
		}
		
		Span<byte> buffer = new(new byte[base64.Length]);
		return Convert.TryFromBase64String(base64, buffer, out int _);
	}

	/// <summary>
	/// Validates strin is content type
	/// </summary>
	/// <param name="value"></param>
	public static bool IsContentType(string value)
	{
		if (value is not string contentType || string.IsNullOrWhiteSpace(contentType))
		{
			return false;
		}

		try
		{
			if (ContentType.TryParse(contentType, out _))
			{
				return true;
			}
		}
		catch (Exception)
		{
			return false;
		}

		return false;
	}

	[GeneratedRegex(@"^[\w,\s-]+\.[\w]+$", RegexOptions.Compiled)]
	private static partial Regex ValidFileNameRegex();
	/// <summary>
	/// Validates file name.
	/// </summary>
	/// <param name="value"></param>
	public static bool IsFileName(string value)
	{
		return value is string fileName && ValidFileNameRegex().IsMatch(fileName);
	}
}