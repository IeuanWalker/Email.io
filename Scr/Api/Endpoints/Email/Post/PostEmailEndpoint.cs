using Api.Infrastructure;
using Database.Models;
using Database.Repositories.Email;
using Database.Repositories.Project;
using Database.Repositories.Template;
using Database.Repositories.TemplateVersion;
using Domain.Services.Email;
using Domain.Services.HashId;
using FluentValidation.Results;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using IMapper = AutoMapper.IMapper;

namespace Api.Endpoints.Email.Post;

public class PostEmailEndpoint : Endpoint<RequestModel, ResponseModel>
{
	public IProjectRepository ProjectTbl { get; set; } = null!;
	public ITemplateRepository TemplateTbl { get; set; } = null!;
	public ITemplateVersionRepository TemplateVersionTbl { get; set; } = null!;
	public IEmailService EmailService { get; set; } = null!;
	public IEmailRepository EmailTbl { get; set; } = null!;
	public IBackgroundJobClient JobClient { get; set; } = null!;
	public IHashIdService HashedService { get; set; } = null!;
	public IMapper Mapper { get; set; } = null!;

	public override void Configure()
	{
		Post("email");
		Version(1);
	}

	public override async Task HandleAsync(RequestModel request, CancellationToken ct)
	{
		// Get Ids from hash
		(int projectId, int templateId)? result = HashedService.DecodeProjectAndTemplateId(request.TemplateId);
		if (result is null)
		{
			ValidationFailures.Add(new ValidationFailure(nameof(request.TemplateId), "TemplateId is not valid"));
			await SendErrorsAsync(cancellation: ct);
			return;
		}

		// Get API key from header
		HttpContext.Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out StringValues apiKey);

		// Get template
		var template = await TemplateVersionTbl
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
			if (!await ProjectTbl.Where(x => x.Id.Equals(result.Value.projectId) && x.ApiKey.Equals(apiKey.ToString())).AnyAsync(cancellationToken: ct))
			{
				ValidationFailures.Add(new ValidationFailure(nameof(request.TemplateId), "TemplateId does not match the provided API key"));
				await SendErrorsAsync(cancellation: ct);
				return;
			}

			if (!await TemplateTbl.Where(x => x.Id.Equals(result.Value.templateId) && x.ProjectId.Equals(result.Value.projectId)).AnyAsync(cancellationToken: ct))
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
			constructedEmail = EmailService.ConstructEmail(request.Data, template.Subject, template.Html, template.PlainText);
		}
		catch (ArgumentException ex)
		{
			ValidationFailures.Add(new ValidationFailure(nameof(request.TemplateId), $"Error constructing email: {ex.Message}"));
			await SendErrorsAsync(cancellation: ct);
			return;
		}

		EmailTbl email = Mapper.Map<EmailTbl>(request);
		email.ProjectId = result.Value.projectId;
		email.TemplateId = result.Value.templateId;
		email.Subject = constructedEmail.Subject;
		email.HtmlContent = constructedEmail.HtmlContent;
		email.PlainTextContent = constructedEmail.PlainTextContent;

		await EmailTbl.Add(email);

		try
		{
			email.HangfireId = JobClient.Enqueue<IEmailService>(x => x.SendEmail(email.Id));
			EmailTbl.Update(email);
		}
		catch (Exception)
		{
			// ignore
		}

		await SendAsync(new ResponseModel
		{
			Reference = HashedService.EncodeEmailId(email.Id)
		}, cancellation: ct);
	}
}