using Database.Context;
using Database.Models;
using Domain.Services.HashId;
using Domain.Services.Slug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Account;

public class IndexModel : PageModel
{
	readonly ISlugService _slugService;
	readonly IHashIdService _hashIdService;
	readonly ApplicationDbContext _context;
	public IndexModel(
		ISlugService slugService,
		IHashIdService hashIdService,
		ApplicationDbContext context)
	{
		_slugService = slugService ?? throw new ArgumentNullException(nameof(slugService));
		_hashIdService = hashIdService ?? throw new ArgumentNullException(nameof(hashIdService));
		_context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public UserTbl CurrentUser { get; set; } = new UserTbl();
	public async Task<IActionResult> OnGet(string slug)
    {
		int? id = _hashIdService.DecodeUserId(_slugService.GetIdFromSlug(slug));
		if (id is null)
		{
			return NotFound();
		}

		UserTbl? user = await _context.UserTbl.FindAsync(id.Value);

		if (user is null)
		{
			return NotFound();
		}

		CurrentUser = user;

		return Page();
	}
}