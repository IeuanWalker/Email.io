using FastEndpoints.Swagger;
using NSwag;

namespace Api.Infrastructure;

static class SwaggerConfiguration
{
	public static IServiceCollection AddSwagger(this IServiceCollection services, string enviroment)
	{
		services.SwaggerDocument(options =>
		{
			options.DocumentSettings = s =>
			{
				s.Title = $"Email.io ({enviroment})";
				s.Version = "v1";
				s.Description = "REST API endpoints for the Email.io system. For more information - https://github.com/IeuanWalker/Email.io";
				s.AddSecurity(ApiKeyAuthenticationOptions.DefaultScheme, new OpenApiSecurityScheme
				{
					In = OpenApiSecurityApiKeyLocation.Header,
					Name = ApiKeyAuthenticationOptions.HeaderName,
					Type = OpenApiSecuritySchemeType.ApiKey
				});
			};
			options.EnableJWTBearerAuth = false;
			options.MaxEndpointVersion = 1;
		});

		return services;
	}

	public static IApplicationBuilder UseSwagger(this IApplicationBuilder app)
	{
		app.UseSwaggerGen(uiConfig: c => c.Path = string.Empty);

		return app;
	}
}