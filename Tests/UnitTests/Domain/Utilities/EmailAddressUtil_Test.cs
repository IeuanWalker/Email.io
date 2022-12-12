using Domain.Utilities;

namespace UnitTests.Domain.Utilities;

public class EmailAddressUtil_Test
{
	[Theory]
	[InlineData("user@domain.com")]
	[InlineData("user@subdomain.domain.com")]
	[InlineData("user+extension@domain.com")]
	[InlineData("user.name@domain.com")]
	[InlineData("user_name@domain.com")]
	[InlineData("test+user_name@domain.com")]
	[InlineData("user@domain.co.uk")]
	[InlineData("user@domain.info")]
	[InlineData("user@domain.museum")]
	[InlineData("user@domain.name")]
	[InlineData("user@domain.net")]
	[InlineData("user@domain.org")]
	[InlineData("user@domain.travel")]
	[InlineData("user@xn--domain-cua.com")]
	[InlineData("user@xn--domain-r6a.com")]
	[InlineData("user@123.45.67.89")]
	[InlineData("12345@192.0.2.235")]
	[InlineData("user@[192.0.2.1]")]
	[InlineData("user@[IPv6:2001:db8:1ff::a0b:dbd0]")]
	[InlineData("user@[IPv6:2001:db8:1ff::a0b:dbd0]:80")]
	[InlineData("!#$%&'*+-/=?^_`{|}~@example.com")]
	[InlineData("\"username\"@example.com")]
	[InlineData("\"john doe\"@example.com")]
	public void IsValidEmailAddress_ValidEmails_ReturnTrue(string email)
	{
		// Act
		bool result = EmailAddressUtil.IsValidEmailAddress(email);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("\t")]
	[InlineData("abc")]
	[InlineData("123")]
	[InlineData("user@examplecom")]
	[InlineData("user@.com")]
	[InlineData("@example.com")]
	[InlineData("user@example@example.com")]
	[InlineData("user@-example.com")]
	[InlineData("user@example..com")]
	[InlineData("user@.example.com")]
	[InlineData("user@example.com.")]
	[InlineData("user@")]
	[InlineData("@domain.com")]
	[InlineData("user@domain")]
	[InlineData("user@domain.")]
	[InlineData("@")]
	[InlineData("@.")]
	[InlineData("@.com")]
	[InlineData(".")]
	[InlineData(".com")]
	public void IsValidEmailAddress_InvalidEmails_ReturnFalse(string email)
	{
		// Act
		bool result = EmailAddressUtil.IsValidEmailAddress(email);

		// Assert
		Assert.False(result);
	}

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
	public void IsValidName_ValidCharacters_ReturnsTrue(string name)
	{
		bool result = EmailAddressUtil.IsValidName(name);

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
	public void IsValidName_InvalidCharacters_ReturnsFalse(char c)
	{
		// Act
		bool result1 = EmailAddressUtil.IsValidName(c.ToString());
		bool result2 = EmailAddressUtil.IsValidName($"Example {c} Test");
		bool result3 = EmailAddressUtil.IsValidName($"{c} Example Test");
		bool result4 = EmailAddressUtil.IsValidName($"Example Test {c}");
		bool result5 = EmailAddressUtil.IsValidName($"Exam{c}ple Test");

		// Assert
		Assert.False(result1);
		Assert.False(result2);
		Assert.False(result3);
		Assert.False(result4);
		Assert.False(result5);
	}
}