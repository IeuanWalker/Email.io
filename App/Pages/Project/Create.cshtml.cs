using App.Database.Models;
using App.Database.Repositories.Project;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace App.Pages.Project
{
    public class CreateModel : PageModel
    {
        private readonly IProjectRepository _projectTbl;

        public CreateModel(IProjectRepository projectTbl)
        {
            _projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
        }

        public void OnGet()
        {
        }

        [BindProperty]
        public ProjectTbl Project { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Project = await _projectTbl.Add(Project).ConfigureAwait(false);

            return RedirectToPage("/Project/Details", new { id = Project.Id });
        }
    }
}