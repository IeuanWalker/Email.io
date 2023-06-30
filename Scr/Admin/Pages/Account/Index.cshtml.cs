using Database.Models;
using Database.Repositories.User;
using Domain.Services.HashId;
using Domain.Services.Slug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Account;

public class IndexModel : PageModel
{
	readonly ISlugService _slugService;
	readonly IHashIdService _hashIdService;
	readonly IUserRepository _userTbl;
	public IndexModel(
		ISlugService slugService,
		IHashIdService hashIdService,
		IUserRepository userTbl)
	{
		_slugService = slugService ?? throw new ArgumentNullException(nameof(slugService));
		_hashIdService = hashIdService ?? throw new ArgumentNullException(nameof(hashIdService));
		_userTbl = userTbl ?? throw new ArgumentNullException(nameof(userTbl));
	}

	public UserTbl CurrentUser { get; set; } = new UserTbl();
	public async Task<IActionResult> OnGet(string slug)
    {
		int? id = _hashIdService.DecodeUserId(_slugService.GetIdFromSlug(slug));
		if (id is null)
		{
			return NotFound();
		}

		UserTbl? user = await _userTbl.GetByID(id.Value);
		if (user is null)
		{
			return NotFound();
		}

		CurrentUser = user;

		return Page();
	}
}