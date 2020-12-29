using App.Database.Models;
using App.Database.Repositories.Project;
using App.Database.Repositories.Template;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Project
{
    public class DetailsModel : PageModel
    {
        private readonly IProjectRepository _projectTbl;
        private readonly ITemplateRepository _templateTbl;

        public DetailsModel(IProjectRepository projectTbl, ITemplateRepository templateTbl)
        {
            _projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
            _templateTbl = templateTbl ?? throw new ArgumentNullException(nameof(templateTbl));
        }

        public ProjectTbl Project { get; set; }

        public async Task OnGet(Guid id)
        {
            // TODO: Error handling
            Project = (await _projectTbl.Get(x => x.Id.Equals(id), null, nameof(ProjectTbl.Templates)).ConfigureAwait(false)).Single();
            if(Project == null)
                throw new NullReferenceException(nameof(Project));

            Project.Templates = Project.Templates.OrderBy(x => x.Name).ToList();
            CreateTemplate = new TemplateTbl
            {
                ProjectId = id
            };
            UpdateTemplateName = new UpdateTemplateNameModel
            {
                ProjectId = id
            };
            DeleteTemplate = new DeleteTemplateModel
            {
                ProjectId = id
            };
        }

        [BindProperty]
        public TemplateTbl CreateTemplate { get; set; }
        public async Task<IActionResult> OnPostCreateTemplate()
        {
            // TODO: Error handling
            TemplateTbl result = await _templateTbl.Add(CreateTemplate).ConfigureAwait(false);

            TempData["toastStatus"] = "success";
            TempData["toastMessage"] = $"Template created - {result.Name}";
            TempData["scrollToId"] = $"template-{result.Id}";

            return RedirectToPage("/Project/Details", new { id = CreateTemplate.ProjectId });
        }

        [BindProperty]
        public UpdateTemplateNameModel UpdateTemplateName { get; set; }
        public async Task<IActionResult> OnPostUpdateTemplateName()
        {
            // TODO: Error handling
            TemplateTbl result = await _templateTbl.GetById(UpdateTemplateName.TemplateId);

            if(result == null)
                throw new NullReferenceException();

            if(UpdateTemplateName.ProjectId != result.ProjectId)
                throw new ArgumentException(nameof(UpdateTemplateName.ProjectId));

            result.Name = UpdateTemplateName.Name;
            _templateTbl.Update(result);

            TempData["toastStatus"] = "success";
            TempData["toastMessage"] = $"Template name update - {result.Name}";
            TempData["scrollToId"] = $"template-{result.Id}";

            return RedirectToPage("/Project/Details", new { id = UpdateTemplateName.ProjectId });
        }

        [BindProperty]
        public DeleteTemplateModel DeleteTemplate { get; set; }
        public async Task<IActionResult> OnPostDeleteTemplate()
        {
            // TODO: Error handling
            Guid projectId = await _templateTbl.Query()
                .Where(x => x.Id.Equals(DeleteTemplate.TemplateId))
                .Select(x => x.ProjectId)
                .FirstOrDefaultAsync();

            if (projectId == null)
                throw new NullReferenceException();

            if (DeleteTemplate.ProjectId != projectId)
                throw new ArgumentException(nameof(DeleteTemplate.ProjectId));

            await _templateTbl.Delete(DeleteTemplate.TemplateId);

            TempData["toastStatus"] = "success";
            TempData["toastMessage"] = "Template deleted";

            return RedirectToPage("/Project/Details", new { id = DeleteTemplate.ProjectId });
        }
    }

    public class UpdateTemplateNameModel
    {
        [Required]
        public Guid ProjectId { get; set; }
        [Required]
        public Guid TemplateId { get; set; }
        [Required]
        [MinLength(1)]
        public string Name { get; set; }
    }

    public class DeleteTemplateModel
    {
        [Required]
        public Guid ProjectId { get; set; }
        [Required]
        public Guid TemplateId { get; set; }
    }
}