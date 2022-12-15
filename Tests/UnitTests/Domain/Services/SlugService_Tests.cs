using Domain.Services.Slug;

namespace UnitTests.Domain.Services;

public class SlugService_Tests
{
	readonly ISlugService _sut = new SlugService();

	[Theory]
	[InlineData("This is a test input string", "this-is-a-test-input-string")]
	[InlineData("This is a test input string with !@#$%^&*()_+<>,.?/", "this-is-a-test-input-string-with")]
	[InlineData("This is a test input string with áéíóú", "this-is-a-test-input-string-with-aeiou")]
	[InlineData("  --this is -- a test --  ", "this-is-a-test")]
	public void GenerateSlug_ReturnsExpectedSlug(string text, string expectedSlug)
	{
		// Act
		string result = _sut.GenerateSlug(text);

		// Assert
		result.Should().Be(expectedSlug);
	}

	[Theory]
	[InlineData("This is a test input string", "12345", "this-is-a-test-input-string-12345")]
	[InlineData("This is a test input string with !@#$%^&*()_+<>,.?/", "67890", "this-is-a-test-input-string-with-67890")]
	[InlineData("This is a test input string with áéíóú", "abcde", "this-is-a-test-input-string-with-aeiou-abcde")]
	[InlineData("  --this is -- a test --  ", "asdfasd", "this-is-a-test-asdfasd")]
	public void GenerateSlug_WithId_ReturnsExpectedSlug(string text, string id, string expectedSlug)
	{
		// Act
		string result = _sut.GenerateSlug(text, id);

		// Assert
		result.Should().Be(expectedSlug);
	}

	[Theory]
	[InlineData("this-is-a-test-input-string-12345", "12345")]
	[InlineData("this-is-a-test-input-string-with-67890", "67890")]
	[InlineData("this-is-a-test-input-string-with-aeiou-abcde", "abcde")]
	public void GetIdFromSlug_ReturnsExpectedId(string text, string expectedId)
	{
		// Act
		string result = _sut.GetIdFromSlug(text);

		// Assert
		result.Should().Be(expectedId);
	}
}