﻿using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages;

public class IndexModel : PageModel
{
	readonly ILogger<IndexModel> _logger;

	public IndexModel(ILogger<IndexModel> logger)
	{
		_logger = logger;
	}

	public void OnGet()
	{
		_logger.LogInformation("Index page visited");
	}
}