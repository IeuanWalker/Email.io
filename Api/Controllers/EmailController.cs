using Microsoft.AspNetCore.Mvc;
using EmailApi.Models;
using Database.Repositories.Project;
using Database.Repositories.Template;
using Microsoft.EntityFrameworkCore;
using Database.Models;
using Domain.Services.Email;
using Database.Repositories.TemplateVersion;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using MimeKit;

namespace EmailApi.Controllers;

[ApiController]
[Route("[controller]")]
public class EmailController : Controller
{
	readonly IProjectRepository _projectTbl;
	readonly ITemplateRepository _templateTbl;
	readonly ITemplateVersionRepository _templateVersionTbl;
	readonly IEmailService _emailService;

	public EmailController(
		IProjectRepository projectTbl,
		ITemplateRepository templateTbl,
		ITemplateVersionRepository templateVersionTbl,
		IEmailService emailService)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
		_templateTbl = templateTbl ?? throw new ArgumentNullException(nameof(templateTbl));
		_templateVersionTbl = templateVersionTbl ?? throw new ArgumentNullException(nameof(templateVersionTbl));
		_emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
	}

	// TODO: Authenticate using API Key
	[HttpPost]
	public async Task<IActionResult> SendEmail([FromBody][Required]EmailModel request)
	{
		// Validate ID's
		if (!await _projectTbl.Where(x => x.Id.Equals(request.ProjectId)).AnyAsync())
		{
			return BadRequest($"{nameof(request.ProjectId)}: {request.ProjectId}, does not exist");
		}

		if(!await _templateTbl.Where(x => x.Id.Equals(request.TemplateId) && x.ProjectId.Equals(request.ProjectId)).AnyAsync())
		{
			return BadRequest($"{nameof(request.TemplateId)}: {request.TemplateId}, does not exist for the {nameof(request.ProjectId)}: {request.ProjectId}");
		}

		// Validate template
		TemplateVersionTbl? template = await _templateVersionTbl.Where(x => x.TemplateId.Equals(request.TemplateId) && x.IsActive).FirstOrDefaultAsync();
		
		if(template is null)
		{
			return BadRequest($"No active template found for {nameof(request.TemplateId)}: {request.TemplateId}");
		}
		if (string.IsNullOrEmpty(template.Html))
		{
			return BadRequest($"No html found for {nameof(request.TemplateId)}: {request.TemplateId}");
		}
		if (string.IsNullOrEmpty(template.Subject))
		{
			return BadRequest($"No subject found for {nameof(request.TemplateId)}: {request.TemplateId}");
		}

		// Construct email
		var test = _emailService.ConstructEmail(request.Data, template.Html, template.Subject);

		// TODO: Move to use hangfire instead
		await _emailService.SendEmail(request?.ToAddresses.Select(x => new MailboxAddress(x.Name ?? string.Empty, x.Email)), test.Subject, test.HtmlBody, JsonSerializer.Serialize(request!.Data));

		return Ok();
	}
}
