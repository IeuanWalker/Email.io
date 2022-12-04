﻿using System.IO;
using System.Text.Json.Nodes;
using Database.Models;
using Database.Repositories.Email;
using HandlebarsDotNet;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace Domain.Services.Email;

public class EmailService : IEmailService
{
	readonly IEmailRepository _emailRepository;

	public EmailService(IEmailRepository emailRepository)
	{
		_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
	}

	public async Task SendEmail(IEnumerable<MailboxAddress> toAddresses, IEnumerable<MailboxAddress>? ccAddresses, IEnumerable<MailboxAddress>? bccAddresses, string subject, string htmlContent, string plainTextContent, List<EmailAttachmentTbl>? attachments = null)
	{
		string? mailHost = string.Empty;
		int mailPort = 0;

		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MailHostUrl")))
		{
			mailHost = Environment.GetEnvironmentVariable("MailHostUrl");
		}
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MailPort")))
		{
			mailPort = Convert.ToInt32(Environment.GetEnvironmentVariable("MailPort"));
		}

		MimeMessage message = new();
		message.From.Add(new MailboxAddress("Test", "noreply@test.com"));
		message.To.AddRange(toAddresses);
		if (ccAddresses?.Any() ?? false)
		{
			message.Cc.AddRange(ccAddresses);
		}
		if (bccAddresses?.Any() ?? false)
		{
			message.Bcc.AddRange(bccAddresses);
		}
		message.Subject = subject;

		var bodyBuilder = new BodyBuilder
		{
			HtmlBody = htmlContent,
			TextBody = plainTextContent
		};

		if (attachments?.Any() ?? false)
		{
			foreach (var attachment in attachments)
			{
				bodyBuilder.Attachments.Add(attachment.FileName, Convert.FromBase64String(attachment.Content), ContentType.Parse(attachment.ContentType));
			}
		}

		message.Body = bodyBuilder.ToMessageBody();

		using SmtpClient mailClient = new();
		await mailClient.ConnectAsync(mailHost, mailPort, SecureSocketOptions.None);
		await mailClient.SendAsync(message);
		await mailClient.DisconnectAsync(true);
	}

	// TODO: Use polly
	public async Task SendEmail(int emailId)
	{
		EmailTbl? email = await _emailRepository.Where(x => x.Id == emailId)
			.Include(x => x.ToAddresses)
			.Include(x => x.CCAddresses)
			.Include(x => x.BCCAddresses)
			.Include(x => x.Attachements)
			.FirstOrDefaultAsync();

		if (email is null || email.Sent is not null)
		{
			return;
		}

		await SendEmail(
			email.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Email)),
			email.CCAddresses?.Select(x => new MailboxAddress(x.Name, x.Email)),
			email.BCCAddresses?.Select(x => new MailboxAddress(x.Name, x.Email)),
			email.Subject,
			email.HtmlContent,
			email.PlainTextContent,
			email.Attachements?.ToList());

		email.Sent = DateTime.Now;

		_emailRepository.Update(email);
	}

	public ConstructedEmail ConstructEmail(JsonNode data, string subjectTemplate, string htmlTemplate, string? plainTextTemplate)
	{
		return new ConstructedEmail
		{
			Subject = RunHandleBars(data, subjectTemplate, nameof(ConstructedEmail.Subject)),
			HtmlContent = RunHandleBars(data, htmlTemplate, nameof(ConstructedEmail.HtmlContent)),
			PlainTextContent = RunHandleBars(data, plainTextTemplate, nameof(ConstructedEmail.PlainTextContent)),
		};
	}

	/// <summary>
	/// Combines handlebars templates with data
	/// </summary>
	/// <param name="data"></param>
	/// <param name="template"></param>
	/// <param name="nameOfConstruction"></param>
	/// <returns>Combined handlebars templates with data</returns>
	/// <exception cref="ArgumentException">Thrown on any error</exception>
	static string RunHandleBars(JsonNode data, string? template, string nameOfConstruction)
	{
		if (string.IsNullOrEmpty(template))
		{
			return data.ToJsonString();
		}

		try
		{
			Handlebars.RegisterHelper("ifCond", (output, options, context, arguments) =>
			{
				if (arguments.Length != 3)
				{
					throw new HandlebarsException("{{#StringEqualityBlockHelper}} helper must have exactly 3 arguments");
				}

				string v1 = arguments.At<string>(0);
				string @operator = arguments.At<string>(1);
				string v2 = arguments.At<string>(0);

				switch (@operator)
				{
					case "==":
						if (v1 == v2)
						{
							options.Template(output, context);
						}
						else
						{
							options.Inverse(output, context);
						}
						break;

					case "!=":
						if (v1 != v2)
						{
							options.Template(output, context);
						}
						else
						{
							options.Inverse(output, context);
						}
						break;

					case "<":
						if (Convert.ToDouble(v1) < Convert.ToDouble(v2))
						{
							options.Template(output, context);
						}
						else
						{
							options.Inverse(output, context);
						}
						break;

					case "<=":
						if (Convert.ToDouble(v1) <= Convert.ToDouble(v2))
						{
							options.Template(output, context);
						}
						else
						{
							options.Inverse(output, context);
						}
						break;

					case ">":
						if (Convert.ToDouble(v1) > Convert.ToDouble(v2))
						{
							options.Template(output, context);
						}
						else
						{
							options.Inverse(output, context);
						}
						break;

					case ">=":
						if (Convert.ToDouble(v1) >= Convert.ToDouble(v2))
						{
							options.Template(output, context);
						}
						else
						{
							options.Inverse(output, context);
						}
						break;
				}
			});

			// Compile template
			HandlebarsTemplate<object, object> compiledTemplate = Handlebars.Compile(template);

			// Add data to template
			return compiledTemplate(data);
		}
		catch (Exception)
		{
			throw new ArgumentException($"Error constructing {nameOfConstruction}", nameof(template));
		}
	}
}

public class ConstructedEmail
{
	public string Subject { get; set; } = string.Empty;
	public string HtmlContent { get; set; } = string.Empty;
	public string PlainTextContent { get; set; } = string.Empty;
}