using Database.Repositories.Email;
using Database.Repositories.Project;
using Database.Repositories.Template;
using Database.Repositories.TemplateTestData;
using Database.Repositories.TemplateVersion;
using Domain.Services.ApiKey;
using Domain.Services.Email;
using Domain.Services.HashId;
using Domain.Services.Slug;

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
		services.AddTransient<ITemplateTestDataRepository, TemplateTestDataRepository>();
		services.AddTransient<IEmailRepository, EmailRepository>();
		services.AddTransient<IEmailService, EmailService>();
		services.AddTransient<IApiKeyService, ApiKeyService>();
		services.AddSingleton<IHashIdService, HashIdService>();
		services.AddSingleton<ISlugService, SlugService>();
	}
}