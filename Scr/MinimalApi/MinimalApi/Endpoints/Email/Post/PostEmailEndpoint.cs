﻿using Database.Models;
using Database.Repositories.Email;
using Database.Repositories.Project;
using Database.Repositories.Template;
using Database.Repositories.TemplateVersion;
using Domain.Services.Email;
using Domain.Services.HashId;
using FastEndpoints;
using FluentValidation.Results;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using MinimalApi.Infrastructure;
using IMapper = AutoMapper.IMapper;

namespace MinimalApi.Endpoints.Email.Post;

[HttpPost("/api/email")]
[Authorize]
public class PostEmailEndpoint : Endpoint<RequestModel>
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
	

	public override async Task HandleAsync(RequestModel request, CancellationToken ct)
	{
		// Get Ids from hash
		(int projectId, int templateId)? result = _hashedService.DecodeProjectAndTemplateId(request.TemplateId);
		if (result is null)
		{
			ValidationFailures.Add(new ValidationFailure(nameof(request.TemplateId), "TemplateId is not valid"));
			await SendErrorsAsync(cancellation: ct);
			return;
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
			// Validate ID's
			if (!await _projectTbl.Where(x => x.Id.Equals(result.Value.projectId) && x.ApiKey.Equals(apiKey.ToString())).AnyAsync(cancellationToken: ct))
			{
				ValidationFailures.Add(new ValidationFailure(nameof(request.TemplateId), "TemplateId does not match the provided API key"));
				await SendErrorsAsync(cancellation: ct);
				return;
			}

			if (!await _templateTbl.Where(x => x.Id.Equals(result.Value.templateId) && x.ProjectId.Equals(result.Value.projectId)).AnyAsync(cancellationToken: ct))
			{
				ValidationFailures.Add(new ValidationFailure(nameof(request.TemplateId), "TemplateId does not exist in the matched project"));
				await SendErrorsAsync(cancellation: ct);
				return;
			}

			ValidationFailures.Add(new ValidationFailure(nameof(request.TemplateId), "No active template found for the template"));
			await SendErrorsAsync(cancellation: ct);
			return;
		}

		// Validate template
		if (string.IsNullOrEmpty(template.Html))
		{
			ValidationFailures.Add(new ValidationFailure(nameof(request.TemplateId), "No html template found for the template"));
			await SendErrorsAsync(cancellation: ct);
			return;
		}
		if (string.IsNullOrEmpty(template.Subject))
		{
			ValidationFailures.Add(new ValidationFailure(nameof(request.TemplateId), "No subject template found for the template"));
			await SendErrorsAsync(cancellation: ct);
			return;
		}

		// Construct email
		ConstructedEmail constructedEmail = new();
		try
		{
			constructedEmail = _emailService.ConstructEmail(request.Data, template.Subject, template.Html, template.PlainText);
		}
		catch (ArgumentException ex)
		{
			ValidationFailures.Add(new ValidationFailure(nameof(request.TemplateId), $"Error constructing email: {ex.Message}"));
			await SendErrorsAsync(cancellation: ct);
			return;
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

		await SendAsync(new ResponseModel
		{
			Reference = _hashedService.EncodeEmailId(email.Id)
		}, cancellation: ct);
	}
}