using Domain.Models;
using Domain.Services.HashId;
using Microsoft.Extensions.Options;

namespace UnitTests.Domain.Services;
public class HashIdService_Tests
{
	readonly HashIdService _hashIdService;
	readonly IOptions<HashSettings> _hashSettings;
	public HashIdService_Tests()
	{
		_hashSettings = Options.Create(new HashSettings
		{
			ProjectIdAndTemplateId = new()
			{
				Salt = "This is my salt",
				MinLength = 30
			},
			ProjectId = new()
			{
				Salt = "This is my salt",
				MinLength = 10
			},
			TemplateVersionId = new()
			{
				Salt = "This is my salt",
				MinLength = 10
			},
			EmailId = new()
			{
				Salt = "This is my salt",
				MinLength = 30
			}
		});

		_hashIdService = new(_hashSettings);
	}

	public static IEnumerable<object[]> ProjectIdTemplateIdHashes
	{
		get
		{
			yield return new object[] { 1, 1, "Vo7qn8631eyER9gIWvMOr25KbgXBzm" };
			yield return new object[] { 2, 4, "3L5a1rV8J4bx0xNfjGWjlDKeEMZX7p" };
			yield return new object[] { 99, 25, "DmQLnXoqKOwVGpzmIn061WNp9aB5bj" };
			yield return new object[] { 1337, 42, "yrWYEBMklm8JRDwAhx04bXjg67wxnQ" };
			yield return new object[] { 683, 94108, "WbMeVBamDlvELeikOzeG6nLJArNPY5" };
			yield return new object[] { 21979508, 150649789, "5WK4dxpyR8NQWbEfDzmX1v3jNELwPz" };
		}
	}

	[Theory]
	[MemberData(nameof(ProjectIdTemplateIdHashes))]
	public void EncodeProjectAndTemplateId_EncodesProvidedValues_ReturnCorrectHash(int projectId, int templateId, string expectedHash)
	{
		// Act
		string hashedResult = _hashIdService.EncodeProjectAndTemplateId(projectId, templateId);

		// Assert
		Assert.True(hashedResult.Length >= _hashSettings.Value.ProjectIdAndTemplateId.MinLength);
		Assert.Equal(expectedHash, hashedResult);
	}

	[Theory]
	[MemberData(nameof(ProjectIdTemplateIdHashes))]
	public void DecodeProjectAndTemplateId_DecodesProvidedHashes_ReturnCorrectProjectIdAndTemplateId(int expectedProjectId, int expectedTemplateId, string hash)
	{
		// Act
		(int projectId, int templateId)? decordedResult = _hashIdService.DecodeProjectAndTemplateId(hash);

		// Assert
		Assert.NotNull(decordedResult);

		if (decordedResult is null)
		{
			return;
		}

		Assert.Equal(expectedProjectId, decordedResult.Value.projectId);
		Assert.Equal(expectedTemplateId, decordedResult.Value.templateId);
	}

	[Theory]
	[InlineData("NV,NV")]
	[InlineData("NV")]
	[InlineData("21OjjRK,21OjjRK")]
	[InlineData("21OjjRK")]
	[InlineData("D54yen6")]
	[InlineData("KVO9yy1oO5j")]
	[InlineData("4bNP1L26r")]
	[InlineData("jvNx4BjM5KYjv")]
	[InlineData("5WK4dxpyR8NQWbEfDzmX4v3jNELwPz")]
	public void DecodeProjectAndTemplateId_InvalidHashes_ReturnsNull(string hash)
	{
		// Act
		(int projectId, int templateId)? decordedResult = _hashIdService.DecodeProjectAndTemplateId(hash);

		// Assert
		Assert.Null(decordedResult);
	}


	public static IEnumerable<object[]> ProjectIdHashes
	{
		get
		{
			yield return new object[] { 1, "DVl0QXvmAx" };
			yield return new object[] { 2, "7xgR9L0ELY" };
			yield return new object[] { 99, "kr2GamNGPK" };
			yield return new object[] { 1337, "kPLvdBeROo" };
			yield return new object[] { 94108, "ZX06zep60Y" };
			yield return new object[] { 150649789, "l0Q9yO5L0m" };
		}
	}

	[Theory]
	[MemberData(nameof(ProjectIdHashes))]
	public void EncodeProjectId_EncodesProjectId_ReturnCorrectHash(int projectId, string expectedHash)
	{
		// Act
		string hashedResult = _hashIdService.EncodeProjectId(projectId);

		// Assert
		Assert.True(hashedResult.Length >= _hashSettings.Value.ProjectId.MinLength);
		Assert.Equal(expectedHash, hashedResult);
	}

	[Theory]
	[MemberData(nameof(ProjectIdHashes))]
	public void DecodeProjectId_DecodesProvidedHashes_ReturnCorrectProjectId(int expectedProjectId, string hash)
	{
		// Act
		int? projectId = _hashIdService.DecodeProjectId(hash);

		// Assert
		Assert.Equal(expectedProjectId, projectId);
	}

	[Theory]
	[InlineData("NV,NV")]
	[InlineData("NV")]
	[InlineData("21OjjRK,21OjjRK")]
	[InlineData("21OjjRK")]
	[InlineData("D54yen6")]
	[InlineData("KVO9yy1oO5j")]
	[InlineData("4bNP1L26r")]
	[InlineData("jvNx4BjM5KYjv")]
	[InlineData("5WK4dxpyR8NQWbEfDzmX4v3jNELwPz")]
	[InlineData("l0Q9yO6L0m")]
	public void DecodeProjectId_InvalidHashes_ReturnsNull(string hash)
	{
		// Act
		int? projectId = _hashIdService.DecodeProjectId(hash);

		// Assert
		Assert.Null(projectId);
	}


	public static IEnumerable<object[]> TemplateIdHashes
	{
		get
		{
			yield return new object[] { 1, "DVl0QXvmAx" };
			yield return new object[] { 2, "7xgR9L0ELY" };
			yield return new object[] { 99, "kr2GamNGPK" };
			yield return new object[] { 1337, "kPLvdBeROo" };
			yield return new object[] { 94108, "ZX06zep60Y" };
			yield return new object[] { 150649789, "l0Q9yO5L0m" };
		}
	}

	[Theory]
	[MemberData(nameof(TemplateIdHashes))]
	public void EncodeTemplateVersionId_EncodesTemplateVersionId_ReturnCorrectHash(int templateVersionId, string expectedHash)
	{
		// Act
		string hashedResult = _hashIdService.EncodeTemplateVersionId(templateVersionId);

		// Assert
		Assert.True(hashedResult.Length >= _hashSettings.Value.TemplateVersionId.MinLength);
		Assert.Equal(expectedHash, hashedResult);
	}

	[Theory]
	[MemberData(nameof(ProjectIdHashes))]
	public void EncodeTemplateVersionId_DecodesProvidedHashes_ReturnCorrectTemplateVersionId(int templateVersionId, string hash)
	{
		// Act
		int? projectId = _hashIdService.DecodeTemplateVersionId(hash);

		// Assert
		Assert.Equal(templateVersionId, projectId);
	}

	[Theory]
	[InlineData("NV,NV")]
	[InlineData("NV")]
	[InlineData("21OjjRK,21OjjRK")]
	[InlineData("21OjjRK")]
	[InlineData("D54yen6")]
	[InlineData("KVO9yy1oO5j")]
	[InlineData("4bNP1L26r")]
	[InlineData("jvNx4BjM5KYjv")]
	[InlineData("5WK4dxpyR8NQWbEfDzmX4v3jNELwPz")]
	[InlineData("l0Q9yO6L0m")]
	public void DecodeTemplateVersionId_InvalidHashes_ReturnsNull(string hash)
	{
		// Act
		int? projectId = _hashIdService.DecodeTemplateVersionId(hash);

		// Assert
		Assert.Null(projectId);
	}


	public static IEnumerable<object[]> EmailIdHashes
	{
		get
		{
			yield return new object[] { 1, "K7y3rOzQ4aDVl0QXvmAxpWq5Jk89g6" };
			yield return new object[] { 2, "kypBnm58rZ7xgR9L0ELYdKlzjMNOAW" };
			yield return new object[] { 99, "gEbw5eAM4Lkr2GamNGPK7oZBxOX6DV" };
			yield return new object[] { 1337, "X2rqyAZnQNkPLvdBeROojDMebdaK56" };
			yield return new object[] { 94108, "y85pMwDxarZX06zep60YWJnBd3jqgz" };
			yield return new object[] { 150649789, "y3rOzQ4aDVl0Q9yO5L0mAxpWq5Jk89" };
		}
	}

	[Theory]
	[MemberData(nameof(EmailIdHashes))]
	public void EncodeEmailId_EncodesTemplateVersionId_ReturnCorrectHash(int emailId, string expectedHash)
	{
		// Act
		string hashedResult = _hashIdService.EncodeEmailId(emailId);

		// Assert
		Assert.True(hashedResult.Length >= _hashSettings.Value.EmailId.MinLength);
		Assert.Equal(expectedHash, hashedResult);
	}

	[Theory]
	[MemberData(nameof(EmailIdHashes))]
	public void DecodeEmailId_DecodesProvidedHashes_ReturnCorrectEmailId(int expectedEmailId, string hash)
	{
		// Act
		int? emailId = _hashIdService.DecodeEmailId(hash);

		// Assert
		Assert.Equal(expectedEmailId, emailId);
	}

	[Theory]
	[InlineData("NV,NV")]
	[InlineData("NV")]
	[InlineData("21OjjRK,21OjjRK")]
	[InlineData("21OjjRK")]
	[InlineData("D54yen6")]
	[InlineData("KVO9yy1oO5j")]
	[InlineData("4bNP1L26r")]
	[InlineData("jvNx4BjM5KYjv")]
	[InlineData("5WK4dxpyR8NQWbEfDzmX4v3jNELwPz")]
	[InlineData("l0Q9yO6L0m")]
	public void DecodeEmailId_InvalidHashes_ReturnsNull(string hash)
	{
		// Act
		int? projectId = _hashIdService.DecodeEmailId(hash);

		// Assert
		Assert.Null(projectId);
	}
}
