using Domain.Validation;

namespace UnitTests.Domain.Validation;

public class IsFileNameAttribute_Test
{
	[Theory]
	[InlineData("myfile.txt")]
	[InlineData("my-file.txt")]
	[InlineData("my_file.txt")]
	[InlineData("my file.txt")]
	[InlineData("file.longextension")]
	public void IsValid_ValidName_ReturnsTrue(string fileName)
	{
		// Arrange
		var attribute = new IsFileNameAttribute();

		// Act
		var result = attribute.IsValid(fileName);

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
	public void IsValid_InvalidName_ReturnsFalse(string fileName)
	{
		// Arrange
		var attribute = new IsFileNameAttribute();

		// Act
		var result = attribute.IsValid(fileName);

		// Assert
		Assert.False(result);
	}
}