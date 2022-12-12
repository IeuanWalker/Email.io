using System.Net;
using Api.Infrastructure;
using Database.Models;
using Database.Repositories.Email;
using Database.Repositories.Project;
using Database.Repositories.Template;
using Database.Repositories.TemplateVersion;
using Domain.Services.Email;
using Domain.Services.HashId;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using IMapper = AutoMapper.IMapper;

namespace Api.Endpoints.Email.Post;

public class PostEmailEndpoint : Endpoint<RequestModel, ResponseModel>
{
	readonly IProjectRepository _projectTbl;
	readonly ITemplateRepository _templateTbl;
	readonly ITemplateVersionRepository _templateVersionTbl;
	readonly IEmailService _emailService;
	readonly IEmailRepository _emailTbl;
	readonly IBackgroundJobClient _jobClient;
	readonly IHashIdService _hashedService;
	readonly IMapper _mapper;

	public PostEmailEndpoint(
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

	public override void Configure()
	{
		Post("email");
		Version(1);
	}

	public override async Task HandleAsync(RequestModel request, CancellationToken ct)
	{
		// Get Ids from hash
		(int projectId, int templateId)? result = _hashedService.DecodeProjectAndTemplateId(request.TemplateId);
		if (result is null)
		{
			ThrowError(x => x.TemplateId, "Invalid TemplateId");
		}

		// Get API key from header
		HttpContext.Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out StringValues apiKey);

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
			.FirstOrDefaultAsync(cancellationToken: ct);

		// If template is null, find out why and return 400 Bad Request, with a message why
		if (template is null)
		{
			//! important - If an API gets reset, its possible for them to get passed the authentication code, as it uses a 2h cache
			if (!await _projectTbl.Where(x => x.ApiKey.Equals(apiKey)).AnyAsync(cancellationToken: ct))
			{
				await SendAsync(null!, (int)HttpStatusCode.Unauthorized, ct);
				return;
			}

			// Validate ID's
			if (!await _projectTbl.Where(x => x.Id.Equals(result.Value.projectId) && x.ApiKey.Equals(apiKey.ToString())).AnyAsync(cancellationToken: ct))
			{
				ThrowError(x => x.TemplateId, "TemplateId does not match the provided API key");
			}

			if (!await _templateTbl.Where(x => x.Id.Equals(result.Value.templateId) && x.ProjectId.Equals(result.Value.projectId)).AnyAsync(cancellationToken: ct))
			{
				ThrowError(x => x.TemplateId, "TemplateId does not exist in the matched project");
			}

			ThrowError(x => x.TemplateId, "No active template found for the template");
		}

		// Validate template
		if (string.IsNullOrEmpty(template.Html))
		{
			AddError(x => x.TemplateId, "No html template found for the template");
		}
		if (string.IsNullOrEmpty(template.Subject))
		{
			AddError(x => x.TemplateId, "No subject template found for the template");
		}
		ThrowIfAnyErrors();

		// Construct email
		ConstructedEmail constructedEmail = new();
		try
		{
			constructedEmail = _emailService.ConstructEmail(request.Data, template.Subject, template.Html!, template.PlainText);
		}
		catch (ArgumentException ex)
		{
			ThrowError(x => x.TemplateId, $"Error constructing email: {ex.Message}");
		}

		EmailTbl email = _mapper.Map<EmailTbl>(request);
		email.ProjectId = result.Value.projectId;
		email.TemplateId = result.Value.templateId;
		email.Subject = constructedEmail.Subject;
		email.HtmlContent = constructedEmail.HtmlContent;
		email.PlainTextContent = constructedEmail.PlainTextContent;

		await _emailTbl.Add(email);

		Response = new ResponseModel
		{
			Reference = _hashedService.EncodeEmailId(email.Id)
		};

		try
		{
			email.HangfireId = _jobClient.Enqueue<IEmailService>(x => x.SendEmail(email.Id));
			_emailTbl.Update(email);
		}
		catch (Exception)
		{
			// ignore
		}
	}
}