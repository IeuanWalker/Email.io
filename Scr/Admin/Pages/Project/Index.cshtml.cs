using Database.Models;
using Database.Repositories.Project;
using Database.Repositories.User;
using Domain.Services.HashId;
using Domain.Services.Slug;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Project;

public class IndexModel : PageModel
{
	readonly IProjectRepository _projectTbl;
	readonly IHashIdService _hashIdService;
	readonly ISlugService _slugService;

	public IndexModel(
		IProjectRepository projectTbl,
		IHashIdService hashIdService,
		ISlugService slugService)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
		_hashIdService = hashIdService ?? throw new ArgumentNullException(nameof(hashIdService));
		_slugService = slugService ?? throw new ArgumentNullException(nameof(slugService));
	}
	public List<ProjectResponseModel1> Projects { get; set; } = new List<ProjectResponseModel1>();

	public async Task OnGet()
	{
		IEnumerable<ProjectTbl> result = await _projectTbl.Get(orderBy: x => x.OrderByDescending(a => a.DateModified)).ConfigureAwait(false);

		if (result?.Any() ?? false)
		{
			Projects = result.Select(x => new ProjectResponseModel1
			{
				Id = x.Id,
				DateModified = x.DateModified,
				Name = x.Name,
				SubHeading = x.SubHeading,
				Description = x.Description,
				Tags = x.Tags,
				ApiKey = x.ApiKey
			}).ToList();
			Projects.ForEach(x => x.Slug = _slugService.GenerateSlug(x.Name, _hashIdService.EncodeProjectId(x.Id)));
		}
	}
}

// TODO: Add ability to request permission to create a project

public class ProjectResponseModel1 : ProjectTbl
{
	public string Slug { get; set; } = default!;
}