using Domain.Models;

namespace MinimalApi.Infrastructure;

static class AppSettingsConfiguration
{
	public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<HashSettings>(configuration.GetSection(nameof(HashSettings)));
		return services;
	}
}