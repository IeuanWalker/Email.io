using Domain.Models;

namespace Api.Infrastructure;

static class AppSettingsConfiguration
{
	public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<HashSettings>(configuration.GetSection(nameof(HashSettings)));
	}
}