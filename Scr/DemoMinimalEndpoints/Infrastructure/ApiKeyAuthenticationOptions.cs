using Microsoft.AspNetCore.Authentication;

namespace DemoMinimalEndpoints.Infrastructure;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
	public const string DefaultScheme = "ApiKey";
	public const string HeaderName = "x-api-key";
}