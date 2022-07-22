using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using EmailApi.Models;
using EmailApi.Services;

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
		_emailService.SendEmail(request);
		return Ok();
	}
}
