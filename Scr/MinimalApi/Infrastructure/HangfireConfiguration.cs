using Hangfire;
using Hangfire.Heartbeat;
using Hangfire.RecurringJobAdmin;
using Hangfire.SqlServer;
using MinimalApi.Infrastructure;

namespace MinimalApi.Infrastructure;

static class HangfireConfiguration
{
	public static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection databaseConnections = configuration.GetSection("DatabaseConnections");
		string? dbConnection = databaseConnections.GetValue<string>("EmailDb");

		if (dbConnection is null)
		{
			throw new ArgumentNullException(nameof(configuration), "Missing database connection string");
		}

		// Add Hangfire services
		services.AddHangfire(x =>
		{
			x.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
			x.UseSimpleAssemblyNameTypeSerializer();
			x.UseRecommendedSerializerSettings();
			x.UseSqlServerStorage(dbConnection, new SqlServerStorageOptions
			{
				CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
				SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
				QueuePollInterval = TimeSpan.Zero,
				UseRecommendedIsolationLevel = true,
				UsePageLocksOnDequeue = true,
				DisableGlobalLocks = true
			});
			x.UseHeartbeatPage(TimeSpan.FromSeconds(1));
			x.UseRecurringJobAdmin(typeof(HangfireConfiguration).Assembly);
		});

		// Add the processing server as IHostedService
		services.AddHangfireServer();

		return services;
	}
}