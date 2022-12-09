namespace Domain.Services.BlobStorage;
public interface IBlobStorageService
{
	/// <summary>
	/// Saves image to blob storage
	/// </summary>
	/// <param name="projectId"></param>
	/// <param name="file"></param>
	/// <param name="name"></param>
	Task<Uri> SaveImage(int projectId, byte[] file, string name);
}
