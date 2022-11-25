namespace Domain.Services.Slug;
public interface ISlugService
{
	string GenerateSlug(string text);
	string GenerateSlug(string text, string id);
	string GetIdFromSlug(string slug);
}
