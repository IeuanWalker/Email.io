using Api.Infrastructure;
using Api.Models.AppSettings;
using Hangfire;
using Hangfire.SqlServer;

namespace Api.Infrastructure;

static class HangfireConfiguration
{
	public static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection databaseConnections = configuration.GetSection(nameof(DatabaseConnections));
		string dbConnection = databaseConnections.GetValue<string>(nameof(DatabaseConnections.EmailDb))
			?? throw new ArgumentNullException(nameof(configuration), "Missing database connection string");

		services.AddHangfire(x =>
		{
			x.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
			x.UseSimpleAssemblyNameTypeSerializer();
			x.UseRecommendedSerializerSettings();
			x.UseSqlServerStorage(dbConnection, new SqlServerStorageOptions
			{
				CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
				SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
				QueuePollInterval = TimeSpan.Zero,
				UseRecommendedIsolationLevel = true,
				DisableGlobalLocks = true,
				EnableHeavyMigrations = true
			});
		});

		// Add the processing server as IHostedService
		services.AddHangfireServer();

		return services;
	}
}