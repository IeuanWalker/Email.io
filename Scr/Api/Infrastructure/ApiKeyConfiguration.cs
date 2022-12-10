using Domain.Services.ApiKey;

namespace Api.Infrastructure;

static class ApiKeyConfiguration
{
	public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services)
	{
		services
			.AddScoped<IApiKeyService, ApiKeyService>()
			.AddScoped<ApiKeyAuthenticationHandler>();

		services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme);
		services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
			.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, null);

		return services;
	}

	public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
	{
		app.UseAuthentication();
		app.UseAuthorization();

		return app;
	}
}