global using FastEndpoints;
global using FluentValidation;
using Api.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiKeyAuthentication();
builder.Services.AddFastEndpoints();
builder.Services.AddSwagger(builder.Environment.EnvironmentName);
builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddHangfire(builder.Configuration);
builder.Services.AddDependencies();
builder.Services.AddMemoryCache();

WebApplication app = builder.Build();
app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();
app.UseApiKeyAuthentication();
app.UseFastEndpoints(c =>
{
	c.Endpoints.RoutePrefix = "api";
	c.Versioning.Prefix = "v";
	c.Versioning.DefaultVersion = 1;
	c.Versioning.PrependToRoute = true;
});
app.UseSwagger();

app.Run();