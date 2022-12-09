using System.Linq.Expressions;
using System.Text.Json.Nodes;
using Database.Models;
using Database.Repositories.TemplateVersion;
using Domain.Services.BlobStorage;
using Domain.Services.Handlebars;
using Domain.Services.Thumbnail;
using Moq;

namespace UnitTests.Domain.Services;
public class ThumbnailService_Tests
{
	readonly IThumbnailService _thumbnailService;
	readonly Mock<ITemplateVersionRepository> _templateVersionTbl = new Mock<ITemplateVersionRepository>();
	readonly Mock<IHandlebarsService> _handlebarsService = new Mock<IHandlebarsService>();
	readonly Mock<IBlobStorageService> _blobStorageService = new Mock<IBlobStorageService>();

	public ThumbnailService_Tests()
	{
		_thumbnailService = new ThumbnailService(
			_templateVersionTbl.Object,
			_handlebarsService.Object,
			_blobStorageService.Object);
	}


	[Fact]
	public async Task GenerateThumbnail_ShouldReturnNull_WhenTemplateVersionIsNotFound()
	{
		// Arrange
		const int templateVersionId = 1;

		// Set up mocked or stubbed dependencies to return empty result
		_templateVersionTbl
			.Setup(x => x.Get(
				It.IsAny<Expression<Func<TemplateVersionTbl, bool>>?>(),
				It.IsAny<Func<IQueryable<TemplateVersionTbl>, IOrderedQueryable<TemplateVersionTbl>>?>(), 
				It.IsAny<string>(), 
				It.IsAny<bool>()))
			.ReturnsAsync(Enumerable.Empty<TemplateVersionTbl>().AsQueryable());
		
		// Act
		await _thumbnailService.GenerateThumbnail(templateVersionId);

		// Assert
		// Verify that Update method of _templateVersionTbl was not called
		_templateVersionTbl.Verify(x => x.Update(It.IsAny<TemplateVersionTbl>()), Times.Never());
	}

	[Fact]
	public async Task GenerateThumbnail_ShouldReturnNull_WhenHtmlIsNotFound()
	{
		// Arrange
		const int templateVersionId = 1;

		// Set up mocked or stubbed dependencies to return result with no Html value
		_templateVersionTbl
			.Setup(x => x.Get(
				It.IsAny<Expression<Func<TemplateVersionTbl, bool>>?>(),
				It.IsAny<Func<IQueryable<TemplateVersionTbl>, IOrderedQueryable<TemplateVersionTbl>>?>(),
				It.IsAny<string>(),
				It.IsAny<bool>()))
			.ReturnsAsync(new[]
			{
				new TemplateVersionTbl
				{
					Html = null
				}
			}.AsQueryable());

		// Act
		await _thumbnailService.GenerateThumbnail(templateVersionId);

		// Assert
		// Verify that Update method of _templateVersionTbl was not called
		_templateVersionTbl.Verify(x => x.Update(It.IsAny<TemplateVersionTbl>()), Times.Never());
	}


	[Fact]
	public async Task GenerateThumbnail_ShouldReturnNull_WhenTestDataIsNotFound()
	{
		// Arrange
		const int templateVersionId = 1;

		// Set up mocked or stubbed dependencies to return result with no TestData value
		_templateVersionTbl
			.Setup(x => x.Get(
				It.IsAny<Expression<Func<TemplateVersionTbl, bool>>?>(),
				It.IsAny<Func<IQueryable<TemplateVersionTbl>, IOrderedQueryable<TemplateVersionTbl>>?>(),
				It.IsAny<string>(),
				It.IsAny<bool>()))
			.ReturnsAsync(new[]
			{
				new TemplateVersionTbl()
			}.AsQueryable());

		// Act
		await _thumbnailService.GenerateThumbnail(templateVersionId);

		// Assert
		// Verify that Update method of _templateVersionTbl was not called
		_templateVersionTbl.Verify(x => x.Update(It.IsAny<TemplateVersionTbl>()), Times.Never());
	}



	[Fact]
	public async Task GenerateThumbnail_ShouldSaveBlobUrl()
	{
		// Arrange
		const int templateVersionId = 1;
		const string testData = "{\"name\": \"John Doe\"}";
		const string html = "<html><body><h1>{{name}}</h1></body></html>";
		const string thumbnailImage = "https://mystorageaccount.blob.core.windows.net/thumbnails/Template-1-Version-1-thumbnail.png";
		
		_templateVersionTbl
			.Setup(x => x.Get(It.IsAny<Expression<Func<TemplateVersionTbl, bool>>?>(), It.IsAny<Func<IQueryable<TemplateVersionTbl>, IOrderedQueryable<TemplateVersionTbl>>?>(), It.IsAny<string>(), It.IsAny<bool>()))
			.ReturnsAsync(new List<TemplateVersionTbl>()
			{
				new TemplateVersionTbl()
				{
					Id = templateVersionId,
					Html = html,
					Template = new TemplateTbl()
					{
						ProjectId = 1
					},
					TestData = new List<TemplateTestDataTbl>()
					{
						new TemplateTestDataTbl()
						{
							IsDefault = true,
							Data = testData
						}
					}
				}
			});

		_handlebarsService
			.Setup(service => service.Render(html, It.IsAny<JsonNode>()))
			.Returns("<html><body><h1>John Doe</h1></body></html>");
		
		_blobStorageService
			.Setup(service => service.SaveImage(
				It.IsAny<int>(),
				It.IsAny<byte[]>(),
				It.IsAny<string>()))
			.ReturnsAsync(new Uri(thumbnailImage));

		// Act
		await _thumbnailService.GenerateThumbnail(templateVersionId);

		// Assert
		_templateVersionTbl.Verify(repo => repo.Update(It.Is<TemplateVersionTbl>(version => version.ThumbnailImage == thumbnailImage)), Times.Once());
	}

}
