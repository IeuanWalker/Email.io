namespace Domain.Services.Thumbnail;
public interface IThumbnailService
{
	/// <summary>
	/// Generates template thumbnails
	/// </summary>
	/// <param name="templateVersionId"></param>
	Task GenerateThumbnail(int templateVersionId);
}
