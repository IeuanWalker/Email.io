using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages;

public class IndexModel : PageModel
{
	readonly ILogger<IndexModel> _logger;

	public IndexModel(ILogger<IndexModel> logger)
	{
		_logger = logger;
	}

	public IActionResult OnGet()
	{
		_logger.LogInformation("Index page visited");

		// TODO: If authenticated send to project page, else continue to this page

		return RedirectToPage("/project/index");
	}
}