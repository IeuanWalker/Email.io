using System.ComponentModel.DataAnnotations;
using Api.Infrastructure;
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

namespace EmailApi.Controllers;

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

	public EmailController(
		IProjectRepository projectTbl,
		ITemplateRepository templateTbl,
		ITemplateVersionRepository templateVersionTbl,
		IEmailService emailService,
		IEmailRepository emailTbl,
		IBackgroundJobClient jobClient,
		IHashIdService hashedService)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
		_templateTbl = templateTbl ?? throw new ArgumentNullException(nameof(templateTbl));
		_templateVersionTbl = templateVersionTbl ?? throw new ArgumentNullException(nameof(templateVersionTbl));
		_emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
		_emailTbl = emailTbl ?? throw new ArgumentNullException(nameof(emailTbl));
		_jobClient = jobClient ?? throw new ArgumentNullException(nameof(jobClient));
		_hashedService = hashedService ?? throw new ArgumentNullException(nameof(hashedService));
	}

	// TODO: Handle attachements
	[HttpPost]
	[Authorize]
	public async Task<IActionResult> SendEmail([FromBody][Required] EmailModel request)
	{
		// Get Ids from hash
		(int projectId, int templateId)? result = _hashedService.DecodeProjectAndTemplateId(request.TemplateId);
		if (result is null)
		{
			return BadRequest($"{nameof(request.TemplateId)}: {request.TemplateId}, is not valid");
		}

		// Get API key from header
		Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var apiKey);

		// Validate ID's
		if (!await _projectTbl.Where(x => x.Id.Equals(result.Value.projectId) && x.ApiKey.Equals(apiKey.ToString())).AnyAsync())
		{
			return BadRequest($"{nameof(request.TemplateId)}: {request.TemplateId}, does not match the provided API key");
		}

		if (!await _templateTbl.Where(x => x.Id.Equals(result.Value.templateId) && x.ProjectId.Equals(result.Value.projectId)).AnyAsync())
		{
			return BadRequest($"{nameof(request.TemplateId)}: {request.TemplateId}, does not exist in the matched project");
		}

		// Validate template
		TemplateVersionTbl? template = await _templateVersionTbl.Where(x => x.TemplateId.Equals(result.Value.templateId) && x.IsActive).FirstOrDefaultAsync();

		if (template is null)
		{
			return BadRequest($"No active template found for {nameof(request.TemplateId)}: {request.TemplateId}");
		}
		if (string.IsNullOrEmpty(template.Html))
		{
			return BadRequest($"No html template found for {nameof(request.TemplateId)}: {request.TemplateId}");
		}
		if (string.IsNullOrEmpty(template.Subject))
		{
			return BadRequest($"No subject template found for {nameof(request.TemplateId)}: {request.TemplateId}");
		}

		// Construct email
		ConstructedEmail constructedEmail = new ConstructedEmail();
		try
		{
			constructedEmail = _emailService.ConstructEmail(request.Data, template.Subject, template.Html, template.PlainText);
		}
		catch (ArgumentException ex)
		{
			return BadRequest($"Error constructing email: {ex.Message}");
		}

		// TODO: Allow all the emails to be sent at the template version level
		// TODO: Use automapper
		EmailTbl email = new()
		{
			ProjectId = result.Value.projectId,
			TemplateId = result.Value.templateId,
			Data = request.Data.ToJsonString(),
			ToAddresses = request.ToAddresses?.Select(x => new EmailAddressTbl
			{
				Name = x.Name,
				Email = x.Email
			}).ToList() ?? new(),
			CCAddresses = request.CCAddresses?.Select(x => new EmailAddressTbl
			{
				Name = x.Name,
				Email = x.Email
			}).ToList(),
			BCCAddresses = request.BCCAddresses?.Select(x => new EmailAddressTbl
			{
				Name = x.Name,
				Email = x.Email
			}).ToList(),
			Subject = constructedEmail.Subject,
			HtmlContent = constructedEmail.HtmlContent,
			PlainTextContent = constructedEmail.PlainTextContent,
			Language = request.Language,
		};
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

		return Ok(_hashedService.Encode(email.Id, 30));
	}
}