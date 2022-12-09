using System.Text.Json.Nodes;
using Database.Models;
using Database.Repositories.TemplateVersion;
using Domain.Services.Handlebars;
using CoreHtmlToImage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs.Models;

namespace Domain.Services.Thumbnail;
public class ThumbnailService : IThumbnailService
{
	readonly ITemplateVersionRepository _templateVersionTbl;
	readonly IHandleBarsService _handlebarsService;
	public ThumbnailService(ITemplateVersionRepository templateVersionTbl, IHandleBarsService handlebarsService)
	{
		_templateVersionTbl = templateVersionTbl ?? throw new ArgumentNullException(nameof(templateVersionTbl));
		_handlebarsService = handlebarsService ?? throw new ArgumentNullException(nameof(handlebarsService));
	}
	
	public async Task GenerateThumbnail(int templateVersionId)
	{
		TemplateVersionTbl? version = (await _templateVersionTbl.Get(
			x => x.Id.Equals(templateVersionId),
			null,
			$"{nameof(TemplateVersionTbl.Template)},{nameof(TemplateVersionTbl.TestData)}"))
			.FirstOrDefault();

		if (version is null || version.Html is null)
		{
			return;
		}

		JsonNode? data = JsonNode.Parse(version.TestData.First(x => x.IsDefault).Data);
		if (data is null)
		{
			return;
		}

		// Compile HTML and test data
		string result = _handlebarsService.Render(version.Html, data);

		HtmlConverter converter = new();
		byte[] thumbnail = converter.FromHtmlString(result, 75, ImageFormat.Png, 50);

		Uri thumbnailUri = await SaveImage(version.Template.ProjectId, thumbnail, $"Template-{version.TemplateId}-Version-{version.Id}-thumbnail.png");

		version.ThumbnailImage = thumbnailUri.ToString();
		_templateVersionTbl.Update(version);
	}

	static async Task<Uri> SaveImage(int projectId, byte[] file, string name)
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
