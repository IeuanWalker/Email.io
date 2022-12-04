using Domain.Validation;

namespace TestProject1.Domain.Validation;

public class IsValidNameAttribute_Tests
{
	[Theory]
	[InlineData("Ieuan")]
	[InlineData("Ieuan Walker")]
	[InlineData("IeuanWalker")]
	[InlineData("gamer_WIZZ")]
	[InlineData("Thomas O'Leary")]
	[InlineData("â, ê, î, ô, û, ŵ, ŷ")] // Welsh
	[InlineData("ä, ö, ü, ß, Ä, Ö, Ü")] // German / Deutsch
	[InlineData("á, é, í, ó, ú, ñ, ¡, ¿, Á, É, Í, Ó, Ú, Ñ")] // Spanish / español
	[InlineData("à, â, ç, é, è, ê, ë, î, ï, ô, œ, ù, û, À, Â, Ç, É, È, Ê, Ë, Î, Ï, Ô, Œ, Ù, Û")] // French / français
	[InlineData("à, á, â, ã, ä, ç, é, ê, í, ó, ô, õ, ú, ü, À, Á, Â, Ã, Ä, Ç, É, Ê, Í, Ó, Ô, Õ, Ú, Ü")] // Portugese / português
	[InlineData("á, é, è, á, ì, ò, ó, ù, Á, É, È, À, Ì, Ò, Ó, Ù")] // Italian / italiano
	[InlineData("Å, å, Ä, ä, Ö, ö")] // Swedish/ finish
	[InlineData("Å, å, Æ, æ, Ø, ø")] // Danish/Norwegian
	public void IsValidNameAttribute_ValidEntries(string testName)
	{
		var attribute = new IsValidNameAttribute();
		
		var result = attribute.IsValid(testName);
		
		Assert.True(result);
	}

	[Theory]
	[InlineData("Ieuan @ Walker")]
	public void IsValidNameAttribute_InValidEntries(string testName)
	{
		var attribute = new IsValidNameAttribute();

		var result = attribute.IsValid(testName);

		Assert.False(result);
	}
}