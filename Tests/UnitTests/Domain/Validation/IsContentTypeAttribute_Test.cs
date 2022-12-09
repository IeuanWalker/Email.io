using Domain.Validation;

namespace UnitTests.Domain.Validation;

public class IsContentTypeAttribute_Test
{
	// Common mimme types - https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types
	[Theory]
	[InlineData("audio/aac")]
	[InlineData("application/x-abiword")]
	[InlineData("application/x-freearc")]
	[InlineData("image/avif")]
	[InlineData("video/x-msvideo")]
	[InlineData("application/vnd.amazon.ebook")]
	[InlineData("application/octet-stream")]
	[InlineData("image/bmp")]
	[InlineData("application/x-bzip")]
	[InlineData("application/x-bzip2")]
	[InlineData("application/x-cdf")]
	[InlineData("application/x-csh")]
	[InlineData("text/css")]
	[InlineData("text/csv")]
	[InlineData("application/msword")]
	[InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
	[InlineData("application/vnd.ms-fontobject")]
	[InlineData("application/epub+zip")]
	[InlineData("application/gzip")]
	[InlineData("image/gif")]
	[InlineData("text/html")]
	[InlineData("image/vnd.microsoft.icon")]
	[InlineData("text/calendar")]
	[InlineData("application/java-archive")]
	[InlineData("image/jpeg")]
	[InlineData("application/json")]
	[InlineData("application/ld+json")]
	[InlineData("audio/midi")]
	[InlineData("audio/x-midi")]
	[InlineData("text/javascript")]
	[InlineData("audio/mpeg")]
	[InlineData("video/mp4")]
	[InlineData("video/mpeg")]
	[InlineData("application/vnd.apple.installer+xml")]
	[InlineData("application/vnd.oasis.opendocument.presentation")]
	[InlineData("application/vnd.oasis.opendocument.spreadsheet")]
	[InlineData("application/vnd.oasis.opendocument.text")]
	[InlineData("audio/ogg")]
	[InlineData("video/ogg")]
	[InlineData("application/ogg")]
	[InlineData("audio/opus")]
	[InlineData("font/otf")]
	[InlineData("image/png")]
	[InlineData("application/pdf")]
	[InlineData("application/x-httpd-php")]
	[InlineData("application/vnd.ms-powerpoint")]
	[InlineData("application/vnd.openxmlformats-officedocument.presentationml.presentation")]
	[InlineData("application/vnd.rar")]
	[InlineData("application/rtf")]
	[InlineData("application/x-sh")]
	[InlineData("image/svg+xml")]
	[InlineData("application/x-tar")]
	[InlineData("image/tiff")]
	[InlineData("video/mp2t")]
	[InlineData("font/ttf")]
	[InlineData("text/plain")]
	[InlineData("application/vnd.visio")]
	[InlineData("audio/wav")]
	[InlineData("audio/webm")]
	[InlineData("video/webm")]
	[InlineData("image/webp")]
	[InlineData("font/woff")]
	[InlineData("font/woff2")]
	[InlineData("application/xhtml+xml")]
	[InlineData("application/vnd.ms-excel")]
	[InlineData("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
	[InlineData("application/xml")]
	[InlineData("text/xml")]
	[InlineData("application/atom+xml")]
	[InlineData("application/vnd.mozilla.xul+xml")]
	[InlineData("application/zip")]
	[InlineData("video/3gpp")]
	[InlineData("audio/3gpp")]
	[InlineData("video/3gpp2")]
	[InlineData("audio/3gpp2")]
	[InlineData("application/x-7z-compressed")]
	public void IsValid_ValidContentTypes_ReturnsTrue(string fileName)
	{
		// Arrange
		IsContentTypeAttribute attribute = new();

		// Act
		bool result = attribute.IsValid(fileName);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("\t")]
	[InlineData(@"C:\Windows\System32\notepad.exe")]
	[InlineData(@"my folder\my file.txt")]
	[InlineData("my*file?.txt")]
	[InlineData(@"my""file.txt")]
	[InlineData("my<file.txt")]
	[InlineData("my>file.txt")]
	[InlineData("my|file.txt")]
	[InlineData("file")]
	[InlineData("file.")]
	[InlineData("file#gg.jpg")]
	public void IsValid_InvalidContentTypes_ReturnsFalse(string fileName)
	{
		// Arrange
		IsContentTypeAttribute attribute = new();

		// Act
		bool result = attribute.IsValid(fileName);

		// Assert
		Assert.False(result);
	}
}