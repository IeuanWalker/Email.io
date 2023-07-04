using System.Security.Claims;
using Database.Models;
using Database.Repositories.Project;
using Domain.Services.ApiKey;
using Domain.Services.HashId;
using Domain.Services.Slug;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Project;

[Authorize(Policy = nameof(UserTbl.CanCreateProject))]
public class CreateModel : PageModel
{
	readonly IProjectRepository _projectTbl;
	readonly IApiKeyService _apiKeyService;
	readonly IHashIdService _hashIdService;
	readonly ISlugService _slugService;

	public CreateModel(
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

		return RedirectToPage("/project/details", new { slug = _slugService.GenerateSlug(Project.Name, _hashIdService.EncodeProjectId(Project.Id)) });
	}
}