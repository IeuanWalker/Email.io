﻿using Admin.Services.Email;
using Database.Repositories.Project;
using Database.Repositories.Template;
using Database.Repositories.TemplateVersion;

namespace Admin.Infrastructure;

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