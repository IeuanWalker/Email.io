using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs;

namespace Domain.Services.BlobStorage;
public class BlobStorageService : IBlobStorageService
{
	public async Task<Uri> SaveImage(int projectId, byte[] file, string name)
	{
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
