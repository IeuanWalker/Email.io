using System.Reflection;
using Api.Infrastructure;
using Domain.Services.ApiKey;
using FastEndpoints;
using FastEndpoints.Swagger;
using MinimalApi.Infrastructure;
using NSwag;

var builder = WebApplication.CreateBuilder(args);
AppSettingsConfiguration.ConfigureServices(builder.Services, builder.Configuration);
builder.Services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme);
builder.Services.AddFastEndpoints();
builder.Services.AddSwaggerDoc(settings =>
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
builder.Services.AddAutoMapper(typeof(Program).GetTypeInfo().Assembly);
builder.Services
	.AddMemoryCache()
	.AddScoped<IApiKeyService, ApiKeyService>()
	.AddScoped<ApiKeyAuthenticationHandler>();

InterfaceConfiguration.ConfigureServices(builder.Services);

builder.Services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
	.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, null);

DatabaseConfiguration.ConfigureServices(builder.Services, builder.Configuration);

HangfireConfiguration.ConfigureServices(builder.Services, builder.Configuration);


var app = builder.Build();
app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();


app.Run();