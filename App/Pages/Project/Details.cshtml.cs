using System;
using System.Threading.Tasks;
using App.Database.Models;
using App.Database.Repositories.Project;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Pages.Project
{
    public class DetailsModel : PageModel
    {
        private readonly IProjectRepository _projectTbl;
        public DetailsModel(IProjectRepository projectTbl)
        {
            _projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
        }

        public ProjectTbl Project { get; set; }
        public async Task OnGet(Guid id)
        {
            Project = await _projectTbl.GetById(id);
        }
    }
}