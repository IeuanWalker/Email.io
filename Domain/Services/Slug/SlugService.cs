using System.Text;
using System.Text.RegularExpressions;

namespace Domain.Services.Slug;

public class SlugService : ISlugService
{
	public string GenerateSlug(string text)
	{
		return TextToUrlSlug(text);
	}

	public string GenerateSlug(string text, string id)
	{
		return $"{TextToUrlSlug(text)}-{id}";
	}

	public string GetIdFromSlug(string slug)
	{
		string[] parts = slug.Split('-');
		return parts[parts.Length - 1];
	}

	static string TextToUrlSlug(string text)
	{
		string str = RemoveAccent(text).ToLower();

		str = Regex.Replace(str, @"\.", " "); // replace fullstop with space
		str = Regex.Replace(str, @"[^a-z0-9\s-]", ""); // remove invalid chars
		str = Regex.Replace(str, @"\s+", " ").Trim(); // convert multiple spaces into one space
		str = Regex.Replace(str, @"\s", "-"); // hyphens

		return str;
	}

	static string RemoveAccent(string txt)
	{
		byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(txt);
		return Encoding.ASCII.GetString(bytes);
	}
}