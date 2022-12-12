using System.Reflection;
using AutoMapper;
using Database.Models;
using Database.Repositories.Email;
using Database.Repositories.Project;
using Database.Repositories.Template;
using Database.Repositories.TemplateVersion;
using Domain.Services.Email;
using Domain.Services.HashId;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Infrastructure;
using MinimalApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddHangfire(builder.Configuration);
builder.Services.AddDependencies();
builder.Services.AddApiKeyAuthentication();
builder.Services.AddAutoMapper(typeof(Program).GetTypeInfo().Assembly);
builder.Services.AddMemoryCache();

var app = builder.Build();
app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();
app.UseApiKeyAuthentication();
app.UseSwagger();
app.UseSwaggerUI(c => c.RoutePrefix = string.Empty);

app.MapPost("/api/email", [Authorize] async (IProjectRepository projectTbl,
		ITemplateRepository templateTbl,
		ITemplateVersionRepository templateVersionTbl,
		IEmailService emailService,
		IEmailRepository emailTbl,
		IBackgroundJobClient jobClient,
		IHashIdService hashedService,
		IMapper mapper,
		[FromBody]RequestModel request,
		[FromHeader(Name = "x-api-key")]string apiKey) =>
{
	// Get Ids from hash
	(int projectId, int templateId)? result = hashedService.DecodeProjectAndTemplateId(request.TemplateId);
	if (result is null)
	{
		return Results.BadRequest($"{nameof(request.TemplateId)}: {request.TemplateId}, is not valid");
	}

	// Get template
	var template = await templateVersionTbl
		.Where(x =>
			x.TemplateId.Equals(result.Value.templateId) &&
			x.IsActive &&
			x.Template.ProjectId.Equals(result.Value.projectId) &&
			x.Template.Project.ApiKey.Equals(apiKey))
		.Select(x => new
		{
			x.Html,
			x.PlainText,
			x.Subject,
		})
		.FirstOrDefaultAsync();

	// If template is null, find out why and return 400 Bad Request, with a message why
	if (template is null)
	{
		// Validate ID's
		if (!await projectTbl.Where(x => x.Id.Equals(result.Value.projectId) && x.ApiKey.Equals(apiKey.ToString())).AnyAsync())
		{
			return Results.BadRequest($"{nameof(request.TemplateId)}: {request.TemplateId}, does not match the provided API key");
		}

		if (!await templateTbl.Where(x => x.Id.Equals(result.Value.templateId) && x.ProjectId.Equals(result.Value.projectId)).AnyAsync())
		{
			return Results.BadRequest($"{nameof(request.TemplateId)}: {request.TemplateId}, does not exist in the matched project");
		}

		return Results.BadRequest($"No active template found for {nameof(request.TemplateId)}: {request.TemplateId}");
	}

	// Validate template
	if (string.IsNullOrEmpty(template.Html))
	{
		return Results.BadRequest($"No html template found for {nameof(request.TemplateId)}: {request.TemplateId}");
	}
	if (string.IsNullOrEmpty(template.Subject))
	{
		return Results.BadRequest($"No subject template found for {nameof(request.TemplateId)}: {request.TemplateId}");
	}

	// Construct email
	ConstructedEmail constructedEmail = new();
	try
	{
		constructedEmail = emailService.ConstructEmail(request.Data, template.Subject, template.Html, template.PlainText);
	}
	catch (ArgumentException ex)
	{
		return Results.BadRequest($"Error constructing email: {ex.Message}");
	}

	EmailTbl email = mapper.Map<EmailTbl>(request);
	email.ProjectId = result.Value.projectId;
	email.TemplateId = result.Value.templateId;
	email.Subject = constructedEmail.Subject;
	email.HtmlContent = constructedEmail.HtmlContent;
	email.PlainTextContent = constructedEmail.PlainTextContent;

	await emailTbl.Add(email);

	try
	{
		email.HangfireId = jobClient.Enqueue<IEmailService>(x => x.SendEmail(email.Id));
		emailTbl.Update(email);
	}
	catch (Exception)
	{
		// ignore
	}

	return Results.Ok(new ResponseModel
	{
		Reference = hashedService.EncodeEmailId(email.Id)
	});
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();
