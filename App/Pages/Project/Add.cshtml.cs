using System;
using System.Threading.Tasks;
using App.Database.Models;
using App.Database.Repositories.Project;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Pages.Project
{
    public class AddModel : PageModel
    {
        private readonly IProjectRepository _projectTbl;
        public AddModel(IProjectRepository projectTbl)
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
            Project = await _projectTbl.Add(Project);

            return RedirectToPage("/Project/Details", new { id = Project.Id });
        }
    }
}