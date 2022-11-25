using Database.Models;
using Database.Repositories.Project;
using Domain.Services.ApiKey;
using Domain.Services.HashId;
using Domain.Services.Slug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Project;

public class SettingsModel : PageModel
{
	readonly IProjectRepository _projectTbl;
	readonly IApiKeyService _apiKeyService;
	readonly IHashIdService _hashIdService;
	readonly ISlugService _slugService;

	public SettingsModel(
		IProjectRepository projectTbl, 
		IApiKeyService apiKeyService,
		IHashIdService hashIdService,
		ISlugService slugService)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
		_apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
		_hashIdService = hashIdService ?? throw new ArgumentNullException(nameof(hashIdService));
		_slugService = slugService ?? throw new ArgumentNullException(nameof(slugService));
	}

	[BindProperty]
	public ProjectTbl? Project { get; set; }

	public async Task<IActionResult> OnGet(string slug)
	{
		int? id = _hashIdService.Decode(_slugService.GetIdFromSlug(slug));

		if (id is null)
		{
			return NotFound();
		}

		// TODO: Error handling
		Project = (await _projectTbl.Get(x => x.Id.Equals(id), null, nameof(ProjectTbl.Templates)).ConfigureAwait(false)).Single();
		if (Project == null)
		{
			return NotFound();
		}

		DeleteProjectId = Project.Id;

		return Page();
	}

	public IActionResult OnPostUpdateProject()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (Project is null)
		{
			return Page();
		}

		_projectTbl.Update(Project);

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Project updated";

		return Page();
	}

	[BindProperty]
	public int DeleteProjectId { get; set; }

	public async Task<IActionResult> OnPostDeleteProject()
	{
		Project = await _projectTbl.GetByID(DeleteProjectId);
		if (Project is null)
		{
			return NotFound();
		}

		await _projectTbl.Delete(Project.Id);

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Project deleted";

		return RedirectToPage("/project/index");
	}

	public async Task<JsonResult> OnPutResetApiKey([FromQuery]int projectId)
	{
		var project = await _projectTbl.GetByID(projectId);
		if (project is null)
		{
			var result = new JsonResult(null);
			result.StatusCode = StatusCodes.Status404NotFound;	
			return result;
		}
		
		string apiKey = await _apiKeyService.GenerateUniqueApiKey();

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(projectId), _ => new ProjectTbl
		{
			ApiKey = apiKey
		});

		return new JsonResult(new
		{
			toastStatus = "success",
			toastTitle = "API key reset",
			apiKey
		});
	}
}