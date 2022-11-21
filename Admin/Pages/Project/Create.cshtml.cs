using Database.Models;
using Database.Repositories.Project;
using Domain.Services.ApiKey;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Project;

public class CreateModel : PageModel
{
	readonly IProjectRepository _projectTbl;
	readonly IApiKeyService _apiKeyService;

	public CreateModel(IProjectRepository projectTbl, IApiKeyService apiKeyService)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
		_apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
	}

	public async Task OnGet()
	{
		Project.ApiKey = await _apiKeyService.GenerateUniqueApiKey();
	}

	[BindProperty]
	public ProjectTbl Project { get; set; } = new ProjectTbl();

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		Project = await _projectTbl.Add(Project).ConfigureAwait(false);

		return RedirectToPage("/Project/Details", new { id = Project.Id });
	}
}