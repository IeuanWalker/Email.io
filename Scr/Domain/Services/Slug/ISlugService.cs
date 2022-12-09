namespace Domain.Services.Slug;

public interface ISlugService
{
	/// <summary>
	/// Generates a slug from the given string.
	/// </summary>
	/// <param name="text"></param>
	string GenerateSlug(string text);

	/// <summary>
	/// Generates a slug from the given string and appends the ID.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="id"></param>
	string GenerateSlug(string text, string id);

	/// <summary>
	/// Extracts the ID from the slug.
	/// </summary>
	/// <param name="slug"></param>
	string GetIdFromSlug(string slug);
}