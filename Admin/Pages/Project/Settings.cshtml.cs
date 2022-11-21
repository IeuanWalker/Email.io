using Database.Models;
using Database.Repositories.Project;
using Domain.Services.ApiKey;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Project;

public class SettingsModel : PageModel
{
	readonly IProjectRepository _projectTbl;
	readonly IApiKeyService _apiKeyService;

	public SettingsModel(IProjectRepository projectTbl, IApiKeyService apiKeyService)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
		_apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
	}

	[BindProperty]
	public ProjectTbl? Project { get; set; }

	public async Task OnGet(Guid id)
	{
		// TODO: Error handling
		Project = (await _projectTbl.Get(x => x.Id.Equals(id), null, nameof(ProjectTbl.Templates)).ConfigureAwait(false)).Single();
		if (Project == null)
		{
			throw new NullReferenceException(nameof(Project));
		}

		DeleteProjectId = Project.Id;
		ResetApiKeyProjectId = Project.Id;
	}

	public IActionResult OnPostUpdateProject()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (Project == null)
		{
			return Page();
		}

		_projectTbl.Update(Project);

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Project updated";

		return Page();
	}

	[BindProperty]
	public Guid DeleteProjectId { get; set; }

	public async Task<IActionResult> OnPostDeleteProject()
	{
		// TODO: Error handling
		Project = await _projectTbl.GetByID(DeleteProjectId);
		if (Project == null)
		{
			throw new NullReferenceException(nameof(Project));
		}

		await _projectTbl.Delete(Project.Id);

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Project deleted";

		return RedirectToPage("/Project/index");
	}

	[BindProperty]
	public Guid ResetApiKeyProjectId { get; set; }
	public async Task<IActionResult> OnPostResetApiKey()
	{
		// TODO: Error handling
		Project = await _projectTbl.GetByID(ResetApiKeyProjectId);
		if (Project == null)
		{
			throw new NullReferenceException(nameof(Project));
		}

		string apiKey = await _apiKeyService.GenerateUniqueApiKey();

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(ResetApiKeyProjectId), _ => new ProjectTbl
		{
			ApiKey = apiKey
		});

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "API key reset";

		return RedirectToPage();
	}
}