using Microsoft.AspNetCore.Authentication;

namespace MinimalApi.Infrastructure;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
	public const string DefaultScheme = "ProjectId";
	public const string HeaderName = "x-api-key";
}