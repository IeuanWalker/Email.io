using System.Linq.Dynamic.Core;
using Admin.Pages.Project;
using Database.Models;
using Database.Repositories.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages;

public class UsersModel : PageModel
{
	readonly IUserRepository _userTbl;
	public UsersModel(IUserRepository userTbl)
	{
		_userTbl = userTbl ?? throw new ArgumentNullException(nameof(userTbl));
	}

	public void OnGet()
	{
		// Method intentionally left empty.
	}

	public async Task<JsonResult> OnPostUsers(DataTablesRequest dataTablesRequest)
	{
		int recordsTotal = await _userTbl.Where().CountAsync();

		IQueryable<UserTbl> sentEmailsQuery = _userTbl.Where();

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
