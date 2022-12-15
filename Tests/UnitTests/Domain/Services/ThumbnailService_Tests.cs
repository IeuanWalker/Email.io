using System.Linq.Expressions;
using System.Text.Json.Nodes;
using Database.Models;
using Database.Repositories.TemplateVersion;
using Domain.Services.BlobStorage;
using Domain.Services.Handlebars;
using Domain.Services.Thumbnail;

namespace UnitTests.Domain.Services;

public class ThumbnailService_Tests
{
	readonly IThumbnailService _sut;
	readonly ITemplateVersionRepository _templateVersionTbl = Substitute.For<ITemplateVersionRepository>();
	readonly IHandlebarsService _handlebarsService = Substitute.For<IHandlebarsService>();
	readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();

	public ThumbnailService_Tests()
	{
		_sut = new ThumbnailService(_templateVersionTbl, _handlebarsService, _blobStorageService);
	}

	[Fact]
	public async Task GenerateThumbnail_ShouldReturnNull_WhenTemplateVersionIsNotFound()
	{
		// Arrange
		const int templateVersionId = 1;

		// Set up mocked or stubbed dependencies to return empty result
		_templateVersionTbl
			.Get(Arg.Any<Expression<Func<TemplateVersionTbl, bool>>?>(), null, $"{nameof(TemplateVersionTbl.Template)},{nameof(TemplateVersionTbl.TestData)}")
			.Returns(new List<TemplateVersionTbl>());

		// Act
		await _sut.GenerateThumbnail(templateVersionId);

		// Assert
		// Verify that Update method of _templateVersionTbl was not called
		_templateVersionTbl.DidNotReceive().Update(Arg.Any<TemplateVersionTbl>());
	}

	[Fact]
	public async Task GenerateThumbnail_ShouldReturnNull_WhenHtmlIsNotFound()
	{
		// Arrange
		const int templateVersionId = 1;

		// Set up mocked or stubbed dependencies to return result with no Html value
		_templateVersionTbl
			.Get(Arg.Any<Expression<Func<TemplateVersionTbl, bool>>?>(), null, $"{nameof(TemplateVersionTbl.Template)},{nameof(TemplateVersionTbl.TestData)}")
			.Returns(new[]
			{
				new TemplateVersionTbl
				{
					Html = null
				}
			});

		// Act
		await _sut.GenerateThumbnail(templateVersionId);

		// Assert
		// Verify that Update method of _templateVersionTbl was not called
		_templateVersionTbl.DidNotReceive().Update(Arg.Any<TemplateVersionTbl>());
	}

	[Fact]
	public async Task GenerateThumbnail_ShouldReturnNull_WhenTestDataIsABlankJson()
	{
		// Arrange
		const int templateVersionId = 1;

		// Set up mocked or stubbed dependencies to return result with no TestData value
		_templateVersionTbl
			.Get(Arg.Any<Expression<Func<TemplateVersionTbl, bool>>?>(), null, $"{nameof(TemplateVersionTbl.Template)},{nameof(TemplateVersionTbl.TestData)}")
			.Returns(new[]
			{
				new TemplateVersionTbl
				{
					Html = "Html",
					TestData = new List<TemplateTestDataTbl>{ new() { IsDefault = true, Data = "{}" } }
				}
			});

		// Act
		await _sut.GenerateThumbnail(templateVersionId);

		// Assert
		// Verify that Update method of _templateVersionTbl was not called
		_templateVersionTbl.DidNotReceive().Update(Arg.Any<TemplateVersionTbl>());
	}

	[Fact]
	public async Task GenerateThumbnail_ShouldReturnNull_WhenTestDataIsInvalidAndThrowsException()
	{
		// Arrange
		const int templateVersionId = 1;

		// Set up mocked or stubbed dependencies to return result with no TestData value
		_templateVersionTbl
			.Get(Arg.Any<Expression<Func<TemplateVersionTbl, bool>>?>(), null, $"{nameof(TemplateVersionTbl.Template)},{nameof(TemplateVersionTbl.TestData)}")
			.Returns(new[]
			{
				new TemplateVersionTbl
				{
					Html = "Html",
					TestData = new List<TemplateTestDataTbl>{ new() { IsDefault = true, Data = " " } }
				}
			});

		// Act
		await _sut.GenerateThumbnail(templateVersionId);

		// Assert
		// Verify that Update method of _templateVersionTbl was not called
		_templateVersionTbl.DidNotReceive().Update(Arg.Any<TemplateVersionTbl>());
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
			.Get(Arg.Any<Expression<Func<TemplateVersionTbl, bool>>?>(), null, $"{nameof(TemplateVersionTbl.Template)},{nameof(TemplateVersionTbl.TestData)}")
			.Returns(new List<TemplateVersionTbl>()
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

		_handlebarsService.Render(html, Arg.Any<JsonNode>()).Returns("<html><body><h1>John Doe</h1></body></html>");
		_blobStorageService.SaveImage(1, Arg.Any<byte[]>(), Arg.Any<string>()).Returns(new Uri(thumbnailImage));

		// Act
		await _sut.GenerateThumbnail(templateVersionId);

		// Assert
		_templateVersionTbl.Received().Update(Arg.Is<TemplateVersionTbl>(t => t.ThumbnailImage == thumbnailImage));
	}
}