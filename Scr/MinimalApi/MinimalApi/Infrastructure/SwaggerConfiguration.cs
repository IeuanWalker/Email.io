using Database.Context;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using NSwag;

namespace MinimalApi.Infrastructure;

static class SwaggerConfiguration
{
	public static IServiceCollection AddSwagger(this IServiceCollection services)
	{
		services.AddSwaggerDoc(settings =>
		{
			settings.Title = "Email.io";
			settings.Version = "v1";
			settings.AddAuth(ApiKeyAuthenticationOptions.DefaultScheme, new OpenApiSecurityScheme
			{
				In = OpenApiSecurityApiKeyLocation.Header,
				Name = ApiKeyAuthenticationOptions.HeaderName,
				Type = OpenApiSecuritySchemeType.ApiKey
			});
		}, tagIndex: 2);
		
		return services;
	}

	public static IApplicationBuilder UseSwagger(this IApplicationBuilder app)
	{
		app.UseSwaggerGen();

		return app;
	}
}
