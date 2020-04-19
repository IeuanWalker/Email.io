using System;
using System.Threading.Tasks;
using App.Database.Models;
using App.Database.Repositories.Template;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Pages.Template
{
    public class AddModel : PageModel
    {
        private readonly ITemplateRepository _templateTbl;
        public AddModel(ITemplateRepository templateTbl)
        {
            _templateTbl = templateTbl ?? throw new ArgumentNullException(nameof(templateTbl));
        }
        public void OnGet()
        {

        }

        public TemplateTbl Template { get; set; }
        public async Task<IActionResult> OnPost()
        {
            await _templateTbl.Add(Template);

            return RedirectToPage("/Project/Details", new { id = Template.ProjectId });
        }
    }
}