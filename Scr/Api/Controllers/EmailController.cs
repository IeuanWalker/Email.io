using Api.Infrastructure;
using AutoMapper;
using Database.Models;
using Database.Repositories.Email;
using Database.Repositories.Project;
using Database.Repositories.Template;
using Database.Repositories.TemplateVersion;
using Domain.Models;
using Domain.Services.Email;
using Domain.Services.HashId;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class EmailController : Controller
{
	readonly IProjectRepository _projectTbl;
	readonly ITemplateRepository _templateTbl;
	readonly ITemplateVersionRepository _templateVersionTbl;
	readonly IEmailService _emailService;
	readonly IEmailRepository _emailTbl;
	readonly IBackgroundJobClient _jobClient;
	readonly IHashIdService _hashedService;
	readonly IMapper _mapper;

	public EmailController(
		IProjectRepository projectTbl,
		ITemplateRepository templateTbl,
		ITemplateVersionRepository templateVersionTbl,
		IEmailService emailService,
		IEmailRepository emailTbl,
		IBackgroundJobClient jobClient,
		IHashIdService hashedService,
		IMapper mapper)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
		_templateTbl = templateTbl ?? throw new ArgumentNullException(nameof(templateTbl));
		_templateVersionTbl = templateVersionTbl ?? throw new ArgumentNullException(nameof(templateVersionTbl));
		_emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
		_emailTbl = emailTbl ?? throw new ArgumentNullException(nameof(emailTbl));
		_jobClient = jobClient ?? throw new ArgumentNullException(nameof(jobClient));
		_hashedService = hashedService ?? throw new ArgumentNullException(nameof(hashedService));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
	}

	// TODO: Handle no email addresses
	[HttpPost]
	[Authorize]
	public async Task<IActionResult> SendEmail([FromBody] EmailModel request)
	{
		// Get Ids from hash
		(int projectId, int templateId)? result = _hashedService.DecodeProjectAndTemplateId(request.TemplateId);
		if (result is null)
		{
			return BadRequest($"{nameof(request.TemplateId)}: {request.TemplateId}, is not valid");
		}

		// Get API key from header
		Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out Microsoft.Extensions.Primitives.StringValues apiKey);

		// Get template
		var template = await _templateVersionTbl
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
			if (!await _projectTbl.Where(x => x.Id.Equals(result.Value.projectId) && x.ApiKey.Equals(apiKey.ToString())).AnyAsync())
			{
				return BadRequest($"{nameof(request.TemplateId)}: {request.TemplateId}, does not match the provided API key");
			}

			#pragma warning disable IDE0046 // Convert to conditional expression
			if (!await _templateTbl.Where(x => x.Id.Equals(result.Value.templateId) && x.ProjectId.Equals(result.Value.projectId)).AnyAsync())
			{
				return BadRequest($"{nameof(request.TemplateId)}: {request.TemplateId}, does not exist in the matched project");
			}

			return BadRequest($"No active template found for {nameof(request.TemplateId)}: {request.TemplateId}");
		}

		// Validate template
		if (string.IsNullOrEmpty(template.Html))
		{
			return BadRequest($"No html template found for {nameof(request.TemplateId)}: {request.TemplateId}");
		}
		if (string.IsNullOrEmpty(template.Subject))
		{
			return BadRequest($"No subject template found for {nameof(request.TemplateId)}: {request.TemplateId}");
		}

		// Construct email
		ConstructedEmail constructedEmail = new();
		try
		{
			constructedEmail = _emailService.ConstructEmail(request.Data, template.Subject, template.Html, template.PlainText);
		}
		catch (ArgumentException ex)
		{
			return BadRequest($"Error constructing email: {ex.Message}");
		}

		EmailTbl email = _mapper.Map<EmailTbl>(request);
		email.ProjectId = result.Value.projectId;
		email.TemplateId = result.Value.templateId;
		email.Subject = constructedEmail.Subject;
		email.HtmlContent = constructedEmail.HtmlContent;
		email.PlainTextContent = constructedEmail.PlainTextContent;

		await _emailTbl.Add(email);

		try
		{
			email.HangfireId = _jobClient.Enqueue<IEmailService>(x => x.SendEmail(email.Id));
			_emailTbl.Update(email);
		}
		catch (Exception)
		{
			// ignore
		}

		return Ok(_hashedService.EncodeEmailId(email.Id));
	}
}