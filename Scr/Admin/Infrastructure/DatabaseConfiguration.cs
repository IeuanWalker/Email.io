using Admin.Models.AppSettings;
using Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure;

static class DatabaseConfiguration
{
	/// <summary>
	///     DbContext settings
	/// </summary>
	/// <param name="services"></param>
	/// <param name="configuration"></param>
	// TODO: Moved to shared project
	public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection databaseConnections = configuration.GetSection(nameof(DatabaseConnections));
		string? connection = databaseConnections.GetValue<string>(nameof(DatabaseConnections.EmailDb));

		if (connection is null)
		{
			throw new ArgumentNullException(nameof(configuration), "Missing database connection string");
		}

		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlServer(connection, b => b.MigrationsAssembly(nameof(Database))));
	}

	// TODO: Fix
	public static void Configure(IApplicationBuilder app)
	{
		try
		{
			using IServiceScope? scope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
			Console.WriteLine("Applying migrations");

			scope?.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();

			Console.WriteLine("Migrations applied");
		}
		catch (Exception)
		{
#pragma warning disable S112 // General exceptions should never be thrown
			throw new ApplicationException("Database migration need to be applied manually");
		}
	}
}