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
	public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection databaseConnections = configuration.GetSection(nameof(DatabaseConnections));
		string connection = databaseConnections.GetValue<string>(nameof(DatabaseConnections.EmailDb));
		
		services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection, b => b.MigrationsAssembly(nameof(Database))));
	}
}