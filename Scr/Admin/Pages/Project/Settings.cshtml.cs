using System.Linq.Dynamic.Core;
using Database.Models;
using Database.Repositories.Email;
using Database.Repositories.Project;
using Domain.Services.ApiKey;
using Domain.Services.HashId;
using Domain.Services.Slug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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
	public ProjectTbl Project { get; set; } = default!;

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
		ProjectTbl? project = await _projectTbl.GetByID(DeleteProjectId);
		if (project is null)
		{
			return NotFound();
		}

		await _projectTbl.Delete(project.Id);

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

	public async Task<JsonResult> OnPostSentEmails(SentEmailsDataTablesRequest dataTablesRequest)
	{
		int recordsTotal = await _emailTbl.Where(x => x.ProjectId.Equals(dataTablesRequest.ProjectId)).CountAsync();

		IQueryable<EmailTbl> sentEmailsQuery = _emailTbl.Where();

		// TODO: Add template filtering

		if (!string.IsNullOrWhiteSpace(dataTablesRequest.Search.Value))
		{
			foreach (string word in dataTablesRequest.Search.Value.Split(' '))
			{
				int? emailId = _hashIdService.DecodeEmailId(word);
				if (emailId is not null)
				{
					sentEmailsQuery = sentEmailsQuery.Where(x => x.Id.Equals(emailId));
					break;
				}

				if (word.Contains('@'))
				{
					sentEmailsQuery = sentEmailsQuery.Where(x => x.ToAddresses.Any(e => e.Email.StartsWith(word)));
					break;
				}

				sentEmailsQuery = sentEmailsQuery.Where(x => x.ToAddresses.Any(e => e.Name!.Contains(word)) || x.ToAddresses.Any(e => e.Email.StartsWith(word)));
			}
		}

		int recordsFiltered = await sentEmailsQuery.CountAsync();

		foreach (Order order in dataTablesRequest.Order)
		{
			sentEmailsQuery = sentEmailsQuery.OrderBy($"{dataTablesRequest.Columns[order.Column].Name} {order.Dir.ToLower()}");
		}

		var data = (await sentEmailsQuery
			.Select(x => new
			{
				x.Id,
				x.TemplateId,
				Sent = ((DateTime)x.Sent!).ToString("dd/MM/yyyy hh:mm tt"),
				ToAddresses = string.Join(", ", x.ToAddresses.Select(e => $"{e.Name} ({e.Email})")),
			})
			.Skip(dataTablesRequest.Start)
			.Take(dataTablesRequest.Length)
			.ToListAsync());

		return new JsonResult(new
		{
			dataTablesRequest.Draw,
			RecordsTotal = recordsTotal,
			RecordsFiltered = recordsFiltered,
			Data = data
		});
	}
}

public class SentEmailsDataTablesRequest : DataTablesRequest
{
	public int ProjectId { get; set; }
}

public class DataTablesRequest
{
	public int Draw { get; set; }

	public List<Column> Columns { get; set; } = default!;

	public List<Order> Order { get; set; } = default!;

	public int Start { get; set; }

	public int Length { get; set; }

	public Search Search { get; set; } = default!;
}

public class Column
{
	public string Data { get; set; } = default!;

	public string Name { get; set; } = default!;

	public bool Searchable { get; set; }

	public bool Orderable { get; set; }

	public Search Search { get; set; } = default!;
}

public class Order
{
	public int Column { get; set; }

	public string Dir { get; set; } = default!;
}

public class Search
{
	public string Value { get; set; } = default!;

	public bool IsRegex { get; set; }
}