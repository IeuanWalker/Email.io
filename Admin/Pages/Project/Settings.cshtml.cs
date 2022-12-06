using Admin.Pages.Components.SentEmails;
using Database.Models;
using Database.Repositories.Email;
using Database.Repositories.Project;
using Domain.Services.ApiKey;
using Domain.Services.HashId;
using Domain.Services.Slug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Admin.Pages.Project;

public class SettingsModel : PageModel
{
	readonly IProjectRepository _projectTbl;
	readonly IApiKeyService _apiKeyService;
	readonly IHashIdService _hashIdService;
	readonly ISlugService _slugService;
	readonly IEmailRepository _emailTbl;

	public SettingsModel(
		IProjectRepository projectTbl,
		IApiKeyService apiKeyService,
		IHashIdService hashIdService,
		ISlugService slugService,
		IEmailRepository emailTbl)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
		_apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
		_hashIdService = hashIdService ?? throw new ArgumentNullException(nameof(hashIdService));
		_slugService = slugService ?? throw new ArgumentNullException(nameof(slugService));
		_emailTbl = emailTbl ?? throw new ArgumentNullException(nameof(emailTbl));
	}

	[BindProperty]
	public ProjectTbl? Project { get; set; }

	public async Task<IActionResult> OnGet(string slug)
	{
		int? id = _hashIdService.DecodeProjectId(_slugService.GetIdFromSlug(slug));

		if (id is null)
		{
			return NotFound();
		}

		Project = (await _projectTbl.Get(x => x.Id.Equals(id), null, nameof(ProjectTbl.Templates)).ConfigureAwait(false)).Single();
		if (Project is null)
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

	public async Task<JsonResult> OnPutResetApiKey([FromQuery] int projectId)
	{
		ProjectTbl? project = await _projectTbl.GetByID(projectId);
		if (project is null)
		{
			return new JsonResult(null)
			{
				StatusCode = StatusCodes.Status404NotFound
			};
		}

		project.ApiKey = await _apiKeyService.GenerateUniqueApiKey();
		_projectTbl.Update(project);

		return new JsonResult(new
		{
			toastStatus = "success",
			toastTitle = "API key reset",
			project.ApiKey
		});
	}
	
	//public SentEmailsDataTablesRequest DataTablesRequest { get; set; }
	public async Task<JsonResult> OnPostSentEmails(SentEmailsDataTablesRequest dataTablesRequest)
	{
		var recordsTotal = await _emailTbl.Where(x => x.ProjectId.Equals(dataTablesRequest.ProjectId)).CountAsync();

		var sentEmailsQuery = _emailTbl.Where();

		// TODO: Add email search
		// TODO: Add template filtering

		if (!string.IsNullOrWhiteSpace(dataTablesRequest.Search.Value))
		{
			foreach (string word in dataTablesRequest.Search.Value.Split(' '))
			{
				if (word.Contains('@'))
				{
					sentEmailsQuery = sentEmailsQuery.Where(x => x.ToAddresses.Any(e => e.Email.StartsWith(word)));
					continue;
				}

				int? emailId = _hashIdService.DecodeEmailId(word);

				if (emailId is not null)
				{
					sentEmailsQuery = sentEmailsQuery.Where(x => x.Id.Equals(emailId));
				}
				else
				{
					sentEmailsQuery = sentEmailsQuery.Where(x => x.ToAddresses.Any(e => e.Name.Contains(word)));
				}
			}
		}

		var recordsFiltered = sentEmailsQuery.Count();

		var sortColumnName = dataTablesRequest.Columns.ElementAt(dataTablesRequest.Order.ElementAt(0).Column).Name;
		var sortDirection = dataTablesRequest.Order.ElementAt(0).Dir.ToLower();

		sentEmailsQuery = sentEmailsQuery.OrderBy($"{sortColumnName} {sortDirection}");

		var skip = dataTablesRequest.Start;
		var take = dataTablesRequest.Length;
		var data = (await sentEmailsQuery
			.Select(x => new
			{
				x.Id,
				x.TemplateId,
				x.Sent,
				x.ToAddresses
			})
			.Skip(skip)
			.Take(take)
			.ToListAsync())
			.Select(x => new
			{
				x.Id,
				x.TemplateId,
				Sent = x.Sent?.ToString("dd/MM/yyyy hh:mm tt") ?? string.Empty,
				ToAddresses = string.Join(", ", x.ToAddresses.Select(y => $"{y.Name} ({y.Email})"))
			});

		return new JsonResult(new
		{
			dataTablesRequest.Draw,
			RecordsTotal = recordsTotal,
			RecordsFiltered = recordsFiltered,
			Data = data
		});
	}
}