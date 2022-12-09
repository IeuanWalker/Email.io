using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Domain.Services.BlobStorage;

public class BlobStorageService : IBlobStorageService
{
	public BlobStorageService()
	{
	}

	public async Task<Uri> SaveImage(int projectId, byte[] file, string name)
	{
		// TODO: Upgrade to Stowage for a generic blob storage implementation, once it supports Azurite - https://github.com/aloneguid/stowage/issues/5
		BlobContainerClient blobContainerClient = new(
			"UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite",
			$"project-{projectId.ToString().ToLower()}");
		await blobContainerClient.CreateIfNotExistsAsync();
		await blobContainerClient.SetAccessPolicyAsync(PublicAccessType.Blob);
		await blobContainerClient.DeleteBlobIfExistsAsync(name);

		Stream stream = new MemoryStream(file);
		await blobContainerClient.UploadBlobAsync(name, stream);

		BlobBaseClient client = new("UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite", $"project-{projectId.ToString().ToLower()}", name);

		return client.Uri.AbsoluteUri.Contains("azurite") ?
			new Uri(client.Uri.AbsoluteUri.Replace("azurite", "localhost")) :
			client.Uri;
	}
}