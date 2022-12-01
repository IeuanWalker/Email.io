using System.Data;
using Database.Repositories.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Admin.Pages.Components.SentEmails;

[ViewComponent]
public class SentEmailsViewComponent : ViewComponent
{
	readonly IEmailRepository _emailTbl;
	public SentEmailsViewComponent(IEmailRepository emailTbl)
	{
		_emailTbl = emailTbl ?? throw new ArgumentNullException(nameof(emailTbl));
	}

	public async Task<IViewComponentResult> InvokeAsync(int	projectId)
	{
		var test = await _emailTbl.Where(x => x.ProjectId == projectId).Take(5).ToListAsync();

		return View(projectId);
	}

	[BindProperty]
	public SentEmailsDataTablesRequest DataTablesRequest { get; set; }

	public async Task<JsonResult> OnPostAsync()
	{
		var recordsTotal = await _emailTbl.Where(x => x.ProjectId.Equals(DataTablesRequest.ProjectId)).CountAsync();

		var sentEmailsQuery = _emailTbl.Where();

		// TODO: Add email search
		// TODO: Add template filtering


		var recordsFiltered = sentEmailsQuery.Count();

		var sortColumnName = DataTablesRequest.Columns.ElementAt(DataTablesRequest.Order.ElementAt(0).Column).Name;
		var sortDirection = DataTablesRequest.Order.ElementAt(0).Dir.ToLower();

		sentEmailsQuery = sentEmailsQuery.OrderBy($"{sortColumnName} {sortDirection}");

		var skip = DataTablesRequest.Start;
		var take = DataTablesRequest.Length;
		var data = await sentEmailsQuery
			.Skip(skip)
			.Take(take)
			.ToListAsync();

		return new JsonResult(new
		{
			DataTablesRequest.Draw,
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

	public List<Column> Columns { get; set; }

	public List<Order> Order { get; set; }

	public int Start { get; set; }

	public int Length { get; set; }

	public Search Search { get; set; }
}

public class Column
{
	public string Data { get; set; }

	public string Name { get; set; }

	public bool Searchable { get; set; }

	public bool Orderable { get; set; }

	public Search Search { get; set; }
}

public class Order
{
	public int Column { get; set; }

	public string Dir { get; set; }
}

public class Search
{
	public string Value { get; set; }

	public bool IsRegex { get; set; }
}