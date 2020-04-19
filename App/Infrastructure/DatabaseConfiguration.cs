using App.Database.Context;
using App.Models.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Infrastructure
{
    internal static class DatabaseConfiguration
    {
        /// <summary>
        ///     DbContext settings
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection databaseConnections = configuration.GetSection(nameof(DatabaseConnections));
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(databaseConnections.GetValue<string>(nameof(DatabaseConnections.EmailDb)),
                    b => b.MigrationsAssembly("App.Database"));
            });
        }
    }
}
