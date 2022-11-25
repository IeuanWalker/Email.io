using System.ComponentModel.DataAnnotations;
using Database.Models;
using Database.Repositories.Project;
using Database.Repositories.Template;
using Database.Repositories.TemplateVersion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages.Project;

public class DetailsModel : PageModel
{
	readonly IProjectRepository _projectTbl;
	readonly ITemplateRepository _templateTbl;
	readonly ITemplateVersionRepository _templateVersionTbl;

	public DetailsModel(
		IProjectRepository projectTbl,
		ITemplateRepository templateTbl,
		ITemplateVersionRepository templateVersionTbl)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
		_templateTbl = templateTbl ?? throw new ArgumentNullException(nameof(templateTbl));
		_templateVersionTbl = templateVersionTbl ?? throw new ArgumentNullException(nameof(templateVersionTbl));
	}

	public ProjectTbl? Project { get; set; }
	
	public async Task OnGet(int id)
	{
		// TODO: Error handling
		Project = (await _projectTbl.Get(x => x.Id.Equals(id), null, $"{nameof(ProjectTbl.Templates)}, {nameof(ProjectTbl.Templates)}.{nameof(TemplateTbl.Versions)}").ConfigureAwait(false)).Single();
		if (Project == null)
		{
			throw new NullReferenceException(nameof(Project));
		}

		Project.Templates = Project.Templates?.OrderBy(x => x.Name).ToList();
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
		MarkAsActive = new MarkAsActiveModel
		{
			ProjectId = id
		};
		DuplicateTemplateVersion = new DuplicateTemplateVersionModel
		{
			ProjectId = id
		};
		DeleteTemplateVersion = new DeleteTemplateVersionModel
		{
			ProjectId = id
		};
	}

	[BindProperty]
	public TemplateTbl CreateTemplate { get; set; } = new TemplateTbl();

	public async Task<IActionResult> OnPostCreateTemplate()
	{
		// TODO: Error handling
		TemplateTbl result = await _templateTbl.Add(CreateTemplate).ConfigureAwait(false);
		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(MarkAsActive.ProjectId), _ => new ProjectTbl
		{
			DateModified = DateTime.Now
		});

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = $"Template created - {result.Name}";
		TempData["scrollToId"] = $"template-{result.Id}";

		return RedirectToPage("/Project/Details", new { id = CreateTemplate.ProjectId });
	}

	[BindProperty]
	public UpdateTemplateNameModel UpdateTemplateName { get; set; } = new UpdateTemplateNameModel();

	public async Task<IActionResult> OnPostUpdateTemplateName()
	{
		// TODO: Error handling
		TemplateTbl? result = await _templateTbl.GetByID(UpdateTemplateName.TemplateId);

		if (result == null)
		{
			throw new NullReferenceException();
		}

		if (UpdateTemplateName.ProjectId != result.ProjectId)
		{
			throw new ArgumentException(nameof(UpdateTemplateName.ProjectId));
		}

		result.Name = UpdateTemplateName.Name;
		_templateTbl.Update(result);

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(MarkAsActive.ProjectId), _ => new ProjectTbl
		{
			DateModified = DateTime.Now
		});

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = $"Template name update - {result.Name}";
		TempData["scrollToId"] = $"template-{result.Id}";

		return RedirectToPage("/Project/Details", new { id = UpdateTemplateName.ProjectId });
	}

	[BindProperty]
	public DeleteTemplateModel DeleteTemplate { get; set; } = new DeleteTemplateModel();

	public async Task<IActionResult> OnPostDeleteTemplate()
	{
		// TODO: Error handling
		int? projectId = await _templateTbl
			.Where(x => x.Id.Equals(DeleteTemplate.TemplateId))
			.Select(x => x.ProjectId)
			.FirstOrDefaultAsync();

		if (projectId == null)
		{
			throw new NullReferenceException();
		}

		if (DeleteTemplate.ProjectId != projectId)
		{
			throw new ArgumentException(nameof(DeleteTemplate.ProjectId));
		}

		await _templateTbl.Delete(DeleteTemplate.TemplateId);

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(MarkAsActive.ProjectId), _ => new ProjectTbl
		{
			DateModified = DateTime.Now
		});

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Template deleted";

		return RedirectToPage("/Project/Details", new { id = DeleteTemplate.ProjectId });
	}

	[BindProperty]
	public TemplateVersionTbl CreateTemplateVersion { get; set; } = new TemplateVersionTbl();

	public async Task<IActionResult> OnPostCreateTemplateVersion()
	{
		// TODO: Error handling
		TemplateTbl? template = (await _templateTbl.Get(x => x.Id.Equals(CreateTemplateVersion.TemplateId), null, nameof(TemplateTbl.Versions))).FirstOrDefault();

		if (template == null)
		{
			throw new NullReferenceException();
		}

		CreateTemplateVersion.Name = "Untitled name";
		CreateTemplateVersion.Subject = "Default subject";
		CreateTemplateVersion.Html = string.Empty;
		CreateTemplateVersion.TestData = "{}";
		if (!template.Versions?.Any() ?? true)
		{
			CreateTemplateVersion.IsActive = true;
		}

		TemplateVersionTbl result = await _templateVersionTbl.Add(CreateTemplateVersion);

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(MarkAsActive.ProjectId), _ => new ProjectTbl
		{
			DateModified = DateTime.Now
		});

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Template version created";
		TempData["scrollToId"] = $"version-{result.Id}";

		return RedirectToPage("/Project/Details", new { id = template.ProjectId });
	}

	[BindProperty]
	public MarkAsActiveModel MarkAsActive { get; set; } = new MarkAsActiveModel();

	public async Task<IActionResult> OnPostMarkAsActive()
	{
		TemplateVersionTbl? version = (await _templateVersionTbl.Get(x =>
				x.Id.Equals(MarkAsActive.VersionId) &&
				x.TemplateId.Equals(MarkAsActive.TemplateId) &&
				x.Template!.ProjectId.Equals(MarkAsActive.ProjectId)))
			.FirstOrDefault();

		if (version == null)
		{
			throw new NullReferenceException(nameof(version));
		}

		version.IsActive = true;

		_templateVersionTbl.Update(version);

		await _templateVersionTbl.UpdateFromQuery(x =>
			x.IsActive &&
			!x.Id.Equals(version.Id) &&
			x.TemplateId.Equals(version.TemplateId),
			_ =>
			new TemplateVersionTbl
			{
				IsActive = false
			});

		await _templateTbl.UpdateFromQuery(x => x.Id.Equals(MarkAsActive.TemplateId), _ => new TemplateTbl
		{
			DateModified = DateTime.Now
		});
		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(MarkAsActive.ProjectId), _ => new ProjectTbl
		{
			DateModified = DateTime.Now
		});

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Template version marked as active";
		TempData["scrollToId"] = $"version-{version.Id}";

		return RedirectToPage("/Project/Details", new { id = MarkAsActive.ProjectId });
	}

	[BindProperty]
	public DuplicateTemplateVersionModel DuplicateTemplateVersion { get; set; } = new DuplicateTemplateVersionModel();

	public async Task<IActionResult> OnPostDuplicateTemplateVersion()
	{
		// TODO: Error handling
		TemplateVersionTbl? version = (await _templateVersionTbl.Get(x =>
			   x.Id.Equals(DuplicateTemplateVersion.VersionId) &&
			   x.TemplateId.Equals(DuplicateTemplateVersion.TemplateId) &&
			   x.Template!.ProjectId.Equals(DuplicateTemplateVersion.ProjectId)))
		   .FirstOrDefault();

		if (version == null)
		{
			throw new NullReferenceException(nameof(version));
		}

		TemplateVersionTbl result = await _templateVersionTbl.Add(new TemplateVersionTbl
		{
			Name = $"{version.Name}_copy",
			Subject = version.Subject,
			TestData = version.TestData,
			Html = version.Html,
			Categories = version.Categories,
			TemplateId = version.TemplateId
		});

		await _templateTbl.UpdateFromQuery(x => x.Id.Equals(MarkAsActive.TemplateId), _ => new TemplateTbl
		{
			DateModified = DateTime.Now
		});
		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(MarkAsActive.ProjectId), _ => new ProjectTbl
		{
			DateModified = DateTime.Now
		});

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Template version duplicated";
		TempData["scrollToId"] = $"version-{result.Id}";

		return RedirectToPage("/Project/Details", new { id = DuplicateTemplateVersion.ProjectId });
	}

	[BindProperty]
	public DeleteTemplateVersionModel DeleteTemplateVersion { get; set; } = new DeleteTemplateVersionModel();

	public async Task<IActionResult> OnPostDeleteTemplateVersion()
	{
		// TODO: Error handling
		TemplateVersionTbl? version = (await _templateVersionTbl.Get(x =>
				x.Id.Equals(DeleteTemplateVersion.VersionId) &&
				x.TemplateId.Equals(DeleteTemplateVersion.TemplateId) &&
				x.Template!.ProjectId.Equals(DeleteTemplateVersion.ProjectId)))
			.FirstOrDefault();

		if (version == null)
		{
			throw new NullReferenceException();
		}

		await _templateVersionTbl.Delete(DeleteTemplateVersion.VersionId);

		await _templateTbl.UpdateFromQuery(x => x.Id.Equals(MarkAsActive.TemplateId), _ => new TemplateTbl
		{
			DateModified = DateTime.Now
		});
		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(MarkAsActive.ProjectId), _ => new ProjectTbl
		{
			DateModified = DateTime.Now
		});

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Template version deleted";
		TempData["scrollToId"] = $"template-{DeleteTemplateVersion.TemplateId}";

		return RedirectToPage("/Project/Details", new { id = DeleteTemplateVersion.ProjectId });
	}
}

public class UpdateTemplateNameModel
{
	[Required]
	public int ProjectId { get; set; }

	[Required]
	public int TemplateId { get; set; }

	[Required]
	[MinLength(1)]
	public string Name { get; set; } = string.Empty;
}

public class DeleteTemplateModel
{
	[Required]
	public int ProjectId { get; set; }

	[Required]
	public int TemplateId { get; set; }
}

public class MarkAsActiveModel
{
	[Required]
	public int ProjectId { get; set; }

	[Required]
	public int TemplateId { get; set; }

	[Required]
	public int VersionId { get; set; }
}

public class DuplicateTemplateVersionModel
{
	[Required]
	public int ProjectId { get; set; }

	[Required]
	public int TemplateId { get; set; }

	[Required]
	public int VersionId { get; set; }
}

public class DeleteTemplateVersionModel
{
	[Required]
	public int ProjectId { get; set; }

	[Required]
	public int TemplateId { get; set; }

	[Required]
	public int VersionId { get; set; }
}