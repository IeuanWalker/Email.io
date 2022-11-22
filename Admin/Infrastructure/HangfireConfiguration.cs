using Admin.Models.AppSettings;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Hangfire.Heartbeat;
using Hangfire.RecurringJobAdmin;
using Hangfire.SqlServer;

namespace Admin.Infrastructure;

static class HangfireConfiguration
{
	public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection databaseConnections = configuration.GetSection(nameof(DatabaseConnections));
		string dbConnection = databaseConnections.GetValue<string>(nameof(DatabaseConnections.EmailDb));

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
			x.UseRecurringJobAdmin(typeof(Startup).Assembly);
		});

		// Add the processing server as IHostedService
		services.AddHangfireServer();
	}

	public static void Configure(IApplicationBuilder app)
	{
		app.UseHangfireDashboard("/dev/Hangfire", new DashboardOptions()
		{
			Authorization = new[] { new NoAuthFilter() },
			IgnoreAntiforgeryToken = true
		});
	}
}

public class NoAuthFilter : IDashboardAuthorizationFilter
{
	public bool Authorize([NotNull] DashboardContext context)
	{
		return true;
	}
}