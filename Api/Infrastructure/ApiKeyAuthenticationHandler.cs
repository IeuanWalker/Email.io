using System.Security.Claims;
using System.Text.Encodings.Web;
using Domain.Services.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Api.Infrastructure;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
	readonly IApiKeyService _apiKeyService;

	public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IApiKeyService apiKeyService) : base(options, logger, encoder, clock)
	{
		_apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
	}

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var apiKey) || apiKey.Count != 1)
		{
			Logger.LogWarning("An API request was received without the x-api-key header");
			return AuthenticateResult.Fail("Invalid parameters");
		}

		var projectId = await _apiKeyService.GetProjectIdFromApiKey(apiKey!);

		if (projectId is null)
		{
			Logger.LogWarning("An API request was received with an invalid API key {apiKey}", apiKey!);
			return AuthenticateResult.Fail("Invalid parameters");
		}

		Logger.BeginScope("{projectId}", projectId);
		Logger.LogInformation("Client authenticated");

		var claims = new[] { new Claim(ClaimTypes.Name, (projectId?.ToString()) ?? string.Empty) };
		var identity = new ClaimsIdentity(claims, ApiKeyAuthenticationOptions.DefaultScheme);
		var identities = new List<ClaimsIdentity> { identity };
		var principal = new ClaimsPrincipal(identities);
		var ticket = new AuthenticationTicket(principal, ApiKeyAuthenticationOptions.DefaultScheme);

		return AuthenticateResult.Success(ticket);
	}
}