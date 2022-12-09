using Domain.Validation;

namespace UnitTests.Domain.Validation;

public class IsValidNameAttribute_Tests
{
	[Theory]
	[InlineData("Ieuan")]
	[InlineData("Ieuan Walker")]
	[InlineData("IeuanWalker")]
	[InlineData("gamer_WIZZ")]
	[InlineData("Thomas O'Leary")]
	[InlineData("â ê î ô û ŵ ŷ")] // Welsh
	[InlineData("ä ö ü ß Ä Ö Ü")] // German / Deutsch
	[InlineData("á é í ó ú ñ ¡ ¿ Á É Í Ó Ú Ñ")] // Spanish / español
	[InlineData("à â ç é è ê ë î ï ô œ ù û À Â Ç É È Ê Ë Î Ï Ô Œ Ù Û")] // French / français
	[InlineData("à á â ã ä ç é ê í ó ô õ ú ü À Á Â Ã Ä Ç É Ê Í Ó Ô Õ Ú Ü")] // Portugese / português
	[InlineData("á é è á ì ò ó ù Á É È À Ì Ò Ó Ù")] // Italian / italiano
	[InlineData("Å å Ä ä Ö ö")] // Swedish/ finish
	[InlineData("Å å Æ æ Ø ø")] // Danish/Norwegian
	public void IsValid_ValidCharacters_ReturnsTrue(string testName)
	{
		IsValidNameAttribute attribute = new();

		bool result = attribute.IsValid(testName);

		Assert.True(result);
	}

	[Theory]
	[InlineData('^')]
	[InlineData('±')]
	[InlineData('!')]
	[InlineData('@')]
	[InlineData('£')]
	[InlineData('$')]
	[InlineData('%')]
	[InlineData('&')]
	[InlineData('*')]
	[InlineData('+')]
	[InlineData('€')]
	[InlineData('#')]
	[InlineData('¢')]
	[InlineData('§')]
	[InlineData('¶')]
	[InlineData('•')]
	[InlineData('ª')]
	[InlineData('º')]
	[InlineData('«')]
	[InlineData('\\')]
	[InlineData('/')]
	[InlineData('<')]
	[InlineData('>')]
	[InlineData('?')]
	[InlineData(':')]
	[InlineData(';')]
	[InlineData('|')]
	[InlineData('=')]
	[InlineData('.')]
	public void IsValid_InvalidCharacters_ReturnsFalse(char c)
	{
		// Arrange
		IsValidNameAttribute attribute = new();

		// Act
		bool result1 = attribute.IsValid($"Example {c} Test");
		bool result2 = attribute.IsValid($"{c} Example Test");
		bool result3 = attribute.IsValid($"Example Test {c}");
		bool result4 = attribute.IsValid($"Exam{c}ple Test");

		// Assert
		Assert.False(result1);
		Assert.False(result2);
		Assert.False(result3);
		Assert.False(result4);
	}
}