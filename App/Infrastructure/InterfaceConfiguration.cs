using App.Database.Repositories.Project;
using App.Database.Repositories.Template;
using App.Database.Repositories.TemplateVersion;
using App.Services.Email;

namespace App.Infrastructure;

static class InterfaceConfiguration
{
	/// <summary>
	///     Interface mapping
	/// </summary>
	/// <param name="services"></param>
	public static void ConfigureServices(IServiceCollection services)
	{
		services.AddTransient<IProjectRepository, ProjectRepository>();
		services.AddTransient<ITemplateRepository, TemplateRepository>();
		services.AddTransient<ITemplateVersionRepository, TemplateVersionRepository>();
		services.AddSingleton<IEmailService, EmailService>();
	}
}