using Domain.Validation;

namespace TestProject1.Domain.Validation;

public class IsEmailAttribute_Tests
{
	[Theory]
	[InlineData("user@domain.com")]
	[InlineData("user@subdomain.domain.com")]
	[InlineData("user+extension@domain.com")]
	[InlineData("user.name@domain.com")]
	[InlineData("user_name@domain.com")]
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
	[InlineData("user@[192.0.2.1]")]
	[InlineData("user@[IPv6:2001:db8:1ff::a0b:dbd0]")]
	[InlineData("user@[IPv6:2001:db8:1ff::a0b:dbd0]:80")]
	public void IsValid_ValidEmails_ReturnTrue(string email)
	{
		// Arrange
		var attribute = new IsEmailAttribute();

		// Act
		bool result = attribute.IsValid(email);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("\t")]
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
	public void IsValid_InvalidEmails_ReturnFalse(string email)
	{			
		// Arrange
		var attribute = new IsEmailAttribute();

		// Act
		bool result = attribute.IsValid(email);

		// Assert
		Assert.False(result);
	}
}