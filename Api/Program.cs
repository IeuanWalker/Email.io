using Admin.Infrastructure;
using Api.Infrastructure;
using Domain.Services.ApiKey;
using Domain.Services.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme);
	
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
	setup.AddSecurityDefinition(ApiKeyAuthenticationOptions.DefaultScheme, new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Name = ApiKeyAuthenticationOptions.HeaderName,
		Type = SecuritySchemeType.ApiKey
	});

	setup.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = ApiKeyAuthenticationOptions.DefaultScheme
				}
			},
			Array.Empty<string>()
		}
	});
});
builder.Services
	.AddMemoryCache()
	.AddScoped<IApiKeyService, ApiKeyService>()
	.AddScoped<ApiKeyAuthenticationHandler>();

// Interface mapping
InterfaceConfiguration.ConfigureServices(builder.Services);

builder.Services.AddAuthentication()
	.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, null);

// Database
DatabaseConfiguration.ConfigureServices(builder.Services, builder.Configuration);

// Hangfire
HangfireConfiguration.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
		options.RoutePrefix = string.Empty;
	});
}

// Hangfire
HangfireConfiguration.Configure(app);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
