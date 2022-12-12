using Database.Context;
using FastendPointsApi.Models.AppSettings;
using Microsoft.EntityFrameworkCore;

namespace FastendPointsApi.Infrastructure;

static class DatabaseConfiguration
{
	/// <summary>
	///     DbContext settings
	/// </summary>
	/// <param name="services"></param>
	/// <param name="configuration"></param>
	// TODO: Moved to shared project
	public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection databaseConnections = configuration.GetSection(nameof(DatabaseConnections));
		string? connection = databaseConnections.GetValue<string>(nameof(DatabaseConnections.EmailDb));

		if (connection is null)
		{
			throw new ArgumentNullException(nameof(configuration), "Missing database connection string");
		}

		services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection, b => b.MigrationsAssembly(nameof(Database))));

		return services;
	}
}