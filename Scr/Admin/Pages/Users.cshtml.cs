using System.Linq.Dynamic.Core;
using Admin.Pages.Project;
using Database.Context;
using Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages;

public class UsersModel : PageModel
{
	readonly ApplicationDbContext _context;
	public UsersModel(ApplicationDbContext context)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public void OnGet()
	{
		// Method intentionally left empty.
	}

	public async Task<JsonResult> OnPostUsers(DataTablesRequest dataTablesRequest)
	{
		int recordsTotal = await _context.UserTbl.CountAsync();

		IQueryable<UserTbl> sentEmailsQuery = _context.UserTbl.AsQueryable();

		if (!string.IsNullOrWhiteSpace(dataTablesRequest.Search.Value))
		{
			foreach (string word in dataTablesRequest.Search.Value.Split(' '))
			{
				if (word.Contains('@'))
				{
					sentEmailsQuery = sentEmailsQuery.Where(x => x.Email.StartsWith(word));
					break;
				}
				else
				{
					sentEmailsQuery = sentEmailsQuery.Where(x => x.GivenName!.StartsWith(word) || x.FamilyName!.StartsWith(word));
				}
			}
		}

		int recordsFiltered = await sentEmailsQuery.CountAsync();

		sentEmailsQuery.OrderBy(x => x.Role).ThenBy(x => x.DisplayName);

		var data = (await sentEmailsQuery
			.Select(x => new
			{
				x.Id,
				x.Role,
				x.DisplayName,
				x.Email,
				x.Initials,
				x.CanCreateProject
			})
			.Skip(dataTablesRequest.Start)
			.Take(dataTablesRequest.Length)
			.ToListAsync())
			.Select(x => new
			{
				x.Id,
				Role = x.Role.ToString(),
				Name = $"{x.DisplayName} ({x.Email})",
				x.Initials,
				x.CanCreateProject
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
