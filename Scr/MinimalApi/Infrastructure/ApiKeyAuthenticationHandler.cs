using System.Security.Claims;
using System.Text.Encodings.Web;
using Domain.Services.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace MinimalApi.Infrastructure;

/// <summary>
/// https://www.camiloterevinto.com/post/simple-and-secure-api-keys-using-asp-net-core
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
	readonly IApiKeyService _apiKeyService;

	public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IApiKeyService apiKeyService) : base(options, logger, encoder, clock)
	{
		_apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
	}

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out StringValues apiKeys) || apiKeys.Count != 1)
		{
			Logger.LogWarning("An API request was received without the x-api-key header");
			return AuthenticateResult.Fail("Invalid parameters");
		}

		string apiKey = apiKeys[0] ?? string.Empty;

		if (string.IsNullOrWhiteSpace(apiKey))
		{
			Logger.LogWarning("An API request was received with an invalid API key");
			return AuthenticateResult.Fail("Invalid parameters");
		}

		if (!await _apiKeyService.DoesApiKeyExist(apiKey))
		{
			Logger.LogWarning("An API request was received with an invalid API key {apiKey}", apiKey!);
			return AuthenticateResult.Fail("Invalid api key");
		}

		Logger.LogInformation("Api key authenticated");

		Claim[] claims = new[] { new Claim(ClaimTypes.Name, string.Empty) };
		ClaimsIdentity identity = new(claims, ApiKeyAuthenticationOptions.DefaultScheme);
		List<ClaimsIdentity> identities = new() { identity };
		ClaimsPrincipal principal = new(identities);
		AuthenticationTicket ticket = new(principal, ApiKeyAuthenticationOptions.DefaultScheme);

		return AuthenticateResult.Success(ticket);
	}
}