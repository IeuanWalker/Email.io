using System.Text;
using System.Text.RegularExpressions;

namespace Domain.Services.Slug;

public partial class SlugService : ISlugService
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
		return slug.Split('-')[^1];
	}

	[GeneratedRegex(@"[^a-z0-9\s-]", RegexOptions.Compiled)]
	private static partial Regex MatchInValidCharacters();
	[GeneratedRegex(@"\s+", RegexOptions.Compiled)]
	private static partial Regex MatchMultipleSpaces();
	static string TextToUrlSlug(string text)
	{
		text = RemoveAccent(text).ToLower();

		text = text.Replace(".", " "); // Replaces full stop with a space
		text = text.Replace("-", " "); // Replaces full stop with a space
		text = MatchInValidCharacters().Replace(text, string.Empty); // Removes invalid characters
		text = MatchMultipleSpaces().Replace(text, " "); // Replaces multiple spaces with a single space
		text = text.Trim().Replace(" ", "-"); // Replaces spaces with a hyphen

		return text;
	}

	static string RemoveAccent(string txt)
	{
		byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(txt);
		return Encoding.ASCII.GetString(bytes);
	}
}