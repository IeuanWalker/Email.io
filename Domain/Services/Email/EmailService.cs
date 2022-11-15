using System.Text.Json.Nodes;
using HandlebarsDotNet;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Domain.Services.Email;

public class EmailService : IEmailService
{
	public async Task SendEmail(IEnumerable<MailboxAddress> to, string subject, string htmlBody, string textBody)
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
		message.To.AddRange(to);
		message.Subject = subject;

		message.Body = new BodyBuilder
		{
			HtmlBody = htmlBody,
			TextBody = textBody
		}.ToMessageBody();

		using SmtpClient mailClient = new();
		await mailClient.ConnectAsync(mailHost, mailPort, SecureSocketOptions.None);
		await mailClient.SendAsync(message);
		await mailClient.DisconnectAsync(true);
	}

	public ConstructedEmail ConstructEmail(JsonObject data, string templateHtmlBody, string templateSubject)
	{
		return new ConstructedEmail
		{
			Subject = RunHandleBars(data, templateSubject, nameof(ConstructedEmail.Subject)),
			HtmlBody = RunHandleBars(data, templateHtmlBody, nameof(ConstructedEmail.HtmlBody))
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
	static string RunHandleBars(JsonObject data, string template, string nameOfConstruction)
	{
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
	public string HtmlBody { get; set; } = string.Empty;
	public string Subject { get; set; } = string.Empty;
}
