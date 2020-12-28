using App.Database.Repositories.Project;
using App.Database.Repositories.Template;
using Microsoft.Extensions.DependencyInjection;

namespace App.Infrastructure
{
    internal static class InterfaceConfiguration
    {
        /// <summary>
        ///     Interface mapping
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<ITemplateRepository, TemplateRepository>();
        }
    }
}