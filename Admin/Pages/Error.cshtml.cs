using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class ErrorModel : PageModel
{
	public string? RequestId { get; set; }

	public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

	readonly ILogger<ErrorModel> _logger;

	public ErrorModel(ILogger<ErrorModel> logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public void OnGet()
	{
		RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
	}
}