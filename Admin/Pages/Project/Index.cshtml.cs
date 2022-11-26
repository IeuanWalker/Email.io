using AutoMapper;
using Database.Models;
using Database.Repositories.Project;
using Domain.Services.HashId;
using Domain.Services.Slug;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Project;

public class IndexModel : PageModel
{
	readonly IProjectRepository _projectTbl;
	readonly IHashIdService _hashIdService;
	readonly ISlugService _slugService;
	readonly IMapper _mapper;

	public IndexModel(
		IProjectRepository projectTbl,
		IHashIdService hashIdService,
		ISlugService slugService,
		IMapper mapper)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
		_hashIdService = hashIdService ?? throw new ArgumentNullException(nameof(hashIdService));
		_slugService = slugService ?? throw new ArgumentNullException(nameof(slugService));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
	}

	public List<ProjectResponseModel1> Projects { get; set; } = new List<ProjectResponseModel1>();

	public async Task OnGet()
	{
		var result = await _projectTbl.Get(orderBy: x => x.OrderByDescending(a => a.DateModified)).ConfigureAwait(false);

		if (result?.Any() ?? false)
		{
			Projects = _mapper.Map<List<ProjectResponseModel1>>(result);
			Projects.ForEach(x => x.Slug = _slugService.GenerateSlug(x.Name, _hashIdService.Encode(x.Id)));
		}
	}
}

public class ProjectResponseModel1 : ProjectTbl
{
	public string Slug { get; set; } = default!;
}