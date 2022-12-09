using System.Text.Json.Nodes;
using CoreHtmlToImage;
using Database.Models;
using Database.Repositories.TemplateVersion;
using Domain.Services.BlobStorage;
using Domain.Services.Handlebars;

namespace Domain.Services.Thumbnail;

public class ThumbnailService : IThumbnailService
{
	readonly ITemplateVersionRepository _templateVersionTbl;
	readonly IHandlebarsService _handlebarsService;
	readonly IBlobStorageService _blobStorageService;

	public ThumbnailService(
		ITemplateVersionRepository templateVersionTbl,
		IHandlebarsService handlebarsService,
		IBlobStorageService blobStorageService)
	{
		_templateVersionTbl = templateVersionTbl ?? throw new ArgumentNullException(nameof(templateVersionTbl));
		_handlebarsService = handlebarsService ?? throw new ArgumentNullException(nameof(handlebarsService));
		_blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
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

		Uri thumbnailUri = await _blobStorageService.SaveImage(version.Template.ProjectId, thumbnail, $"Template-{version.TemplateId}-Version-{version.Id}-thumbnail.png");

		version.ThumbnailImage = thumbnailUri.ToString();
		_templateVersionTbl.Update(version);
	}
}