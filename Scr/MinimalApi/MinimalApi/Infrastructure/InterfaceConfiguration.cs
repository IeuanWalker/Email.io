using Database.Repositories.Email;
using Database.Repositories.Project;
using Database.Repositories.Template;
using Database.Repositories.TemplateTestData;
using Database.Repositories.TemplateVersion;
using Domain.Services.ApiKey;
using Domain.Services.BlobStorage;
using Domain.Services.Email;
using Domain.Services.Handlebars;
using Domain.Services.HashId;
using Domain.Services.Slug;
using Domain.Services.Thumbnail;

namespace MinimalApi.Infrastructure;

static class InterfaceConfiguration
{
	/// <summary>
	///     Interface mapping
	/// </summary>
	/// <param name="services"></param>
	public static IServiceCollection AddDependencies(this IServiceCollection services)
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
		services.AddSingleton<IHandlebarsService, HandlebarsService>();
		services.AddTransient<IThumbnailService, ThumbnailService>();
		services.AddSingleton<IBlobStorageService, BlobStorageService>();
		return services;
	}
}