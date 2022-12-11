using FastEndpoints.Swagger;
using NSwag;
using NSwag.AspNetCore;

namespace Api.Infrastructure;

static class SwaggerConfiguration
{
	public static IServiceCollection AddSwagger(this IServiceCollection services, string enviroment)
	{
		services.AddSwaggerDoc(settings =>
		{
			settings.Title = $"Email.io ({enviroment})";
			settings.Description = "REST API endpoints for the Email.io system. For more information - https://github.com/IeuanWalker/Email.io";
			settings.Version = "v1";
			settings.AddAuth(ApiKeyAuthenticationOptions.DefaultScheme, new OpenApiSecurityScheme
			{
				In = OpenApiSecurityApiKeyLocation.Header,
				Name = ApiKeyAuthenticationOptions.HeaderName,
				Type = OpenApiSecuritySchemeType.ApiKey
			});
		}, addJWTBearerAuth: false, tagIndex: 1, maxEndpointVersion: 1);

		return services;
	}

	public static IApplicationBuilder UseSwagger(this IApplicationBuilder app)
	{		
		app.UseSwaggerGen(uiConfig: c => c.Path = string.Empty);

		return app;
	}
}