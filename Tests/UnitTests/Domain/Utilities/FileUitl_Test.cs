using Domain.Utilities;

namespace UnitTests.Domain.Utilities;

public class FileUitl_Test
{
	[Fact]
	public void IsBase64String_ShouldReturnTrueForValidBase64String()
	{
		// Arrange
		const string base64 = "Zm9vYmFy";

		// Act
		bool result = FileUtil.IsBase64String(base64);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void IsBase64String_ShouldReturnFalseForInvalidBase64String()
	{
		// Arrange
		const string base64 = "Zm9vYmFy!";

		// Act
		bool result = FileUtil.IsBase64String(base64);

		// Assert
		Assert.False(result);
	}
}