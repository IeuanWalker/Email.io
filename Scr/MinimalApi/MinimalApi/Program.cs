using System.Reflection;
using FastEndpoints;
using MinimalApi.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddFastEndpoints();
builder.Services.AddSwagger();
builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddHangfire(builder.Configuration);
builder.Services.AddDependencies();
builder.Services.AddApiKeyAuthentication();
builder.Services.AddAutoMapper(typeof(Program).GetTypeInfo().Assembly);
builder.Services.AddMemoryCache();

WebApplication app = builder.Build();
app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();
app.UseApiKeyAuthentication();
app.UseFastEndpoints();
app.UseSwagger();

app.Run();