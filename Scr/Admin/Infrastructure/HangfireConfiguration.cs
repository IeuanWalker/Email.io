using Admin.Models.AppSettings;
using Database.Models;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;

namespace Admin.Infrastructure;

// TODO: Moved to shared project
static class HangfireConfiguration
{
	public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
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
	public bool Authorize(DashboardContext context)
	{
		HttpContext httpContext = context.GetHttpContext();

		if (!httpContext.User.Identity?.IsAuthenticated ?? true)
		{
			return false;
		}

		return httpContext.User.IsInRole(nameof(UserRoles.Admin));
	}
}