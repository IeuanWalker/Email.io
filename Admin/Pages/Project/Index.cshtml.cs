using Database.Models;
using Database.Repositories.Project;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Project;

public class IndexModel : PageModel
{
	readonly IProjectRepository _projectTbl;

	public IndexModel(IProjectRepository projectTbl)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
	}

	public IEnumerable<ProjectTbl> Projects { get; set; } = new List<ProjectTbl>();

	public async Task OnGet()
	{
		Projects = await _projectTbl.Get(orderBy: x => x.OrderByDescending(a => a.DateModified)).ConfigureAwait(false);
	}
}