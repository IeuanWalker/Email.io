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

		// TODO: Benchmark
		//StringBuilder slugBuilder = new();
		//slugBuilder.Append(TextToUrlSlug(text));
		//slugBuilder.Append('-');
		//slugBuilder.Append(id);
		//return slugBuilder.ToString();
	}

	public string GetIdFromSlug(string slug)
	{
		return slug.Split('-')[^1];

		// TODO: Benchmark
		//int lastHyphenIndex = slug.LastIndexOf('-');
		//return slug.Substring(lastHyphenIndex + 1);
	}

	[GeneratedRegex(@"[^a-z0-9\s-]", RegexOptions.Compiled)]
	private static partial Regex MatchInValidCharactersRegex();

	[GeneratedRegex(@"[\s._-]+", RegexOptions.Compiled)]
	private static partial Regex ReplaceWithHyphenRegex();

	static string TextToUrlSlug(string text)
	{
		text = text.Normalize(NormalizationForm.FormD).ToLowerInvariant();
		text = ReplaceWithHyphenRegex().Replace(text, "-");
		text = MatchInValidCharactersRegex().Replace(text, string.Empty);
		text = text.Trim('-');

		return text;
	}
}