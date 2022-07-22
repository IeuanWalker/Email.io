using Microsoft.AspNetCore.Mvc;
using MimeKit;
using EmailApi.Models;
using Domain.Services.Email;

namespace EmailApi.Controllers;

[ApiController]
[Route("[controller]")]
public class EmailController : Controller
{
	readonly IEmailService _emailService;

	public EmailController(IEmailService emailService)
	{
		_emailService = emailService;
	}

	[HttpPost]
	public IActionResult SendEmail(EmailModel request)
	{
		List<MailboxAddress> addresses = new();
		foreach (EmailAddresses? address in request.ToAddresses)
		{
			addresses.Add(new MailboxAddress(address.Name, address.Email));
		}

		_emailService.SendEmail(addresses, request.Subject, request.HtmlBody, request.TextBody);
		return Ok();
	}
}
