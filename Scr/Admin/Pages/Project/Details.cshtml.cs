using System.ComponentModel.DataAnnotations;
using Database.Models;
using Database.Repositories.Project;
using Database.Repositories.Template;
using Database.Repositories.TemplateVersion;
using Domain.Services.HashId;
using Domain.Services.Slug;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages.Project;
[Authorize]
public class DetailsModel : PageModel
{
	readonly IProjectRepository _projectTbl;
	readonly ITemplateRepository _templateTbl;
	readonly ITemplateVersionRepository _templateVersionTbl;
	readonly IHashIdService _hashIdService;
	readonly ISlugService _slugService;

	public DetailsModel(
		IProjectRepository projectTbl,
		ITemplateRepository templateTbl,
		ITemplateVersionRepository templateVersionTbl,
		IHashIdService hashIdService,
		ISlugService slugService)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
		_templateTbl = templateTbl ?? throw new ArgumentNullException(nameof(templateTbl));
		_templateVersionTbl = templateVersionTbl ?? throw new ArgumentNullException(nameof(templateVersionTbl));
		_hashIdService = hashIdService ?? throw new ArgumentNullException(nameof(hashIdService));
		_slugService = slugService ?? throw new ArgumentNullException(nameof(slugService));
	}

	public ProjectResponseModel Project { get; set; } = new ProjectResponseModel();

	public async Task<IActionResult> OnGet(string slug)
	{
		int? id = _hashIdService.DecodeProjectId(_slugService.GetIdFromSlug(slug));

		if (id is null)
		{
			return NotFound();
		}

		ProjectTbl? project = (await _projectTbl.Get(x => x.Id.Equals(id), null, $"{nameof(ProjectTbl.Templates)}, {nameof(ProjectTbl.Templates)}.{nameof(TemplateTbl.Versions)}").ConfigureAwait(false)).Single();
		if (Project is null)
		{
			return NotFound();
		}

		Project = new ProjectResponseModel
		{
			Id = project.Id,
			Slug = slug,
			Name = project.Name,
			SubHeading = project.SubHeading,
			Description = project.Description,
			Tags = project.Tags,
			ApiKey = project.ApiKey,
			Templates = project.Templates?.Select(t => new TemplateResponseModel
			{
				Id = t.Id,
				DateModified = t.DateModified,
				HashedApiId = _hashIdService.EncodeProjectAndTemplateId(project.Id, t.Id),
				Name = t.Name,
				ProjectId = t.ProjectId,
				Versions = t.Versions?.Select(v => new TemplateVersionResponseModel
				{
					Id = v.Id,
					HashedId = _hashIdService.EncodeTemplateVersionId(v.Id),
					DateModified = v.DateModified,
					Name = v.Name,
					TemplateNameSlug = _slugService.GenerateSlug(v.Name),
					IsActive = v.IsActive,
					ThumbnailImage = v.ThumbnailImage
				})
				.OrderByDescending(x => x.IsActive)
				.ThenByDescending(x => x.DateModified)
				.ToList()
			})
			.OrderBy(x => x.Name)
			.ToList()
		};

		CreateTemplate = new TemplateTbl
		{
			ProjectId = Project.Id
		};
		UpdateTemplateName = new UpdateTemplateNameModel
		{
			ProjectId = Project.Id
		};
		DeleteTemplate = new DeleteTemplateModel
		{
			ProjectId = Project.Id
		};
		MarkAsActive = new MarkAsActiveModel
		{
			ProjectId = Project.Id
		};
		DuplicateTemplateVersion = new DuplicateTemplateVersionModel
		{
			ProjectId = Project.Id
		};
		DeleteTemplateVersion = new DeleteTemplateVersionModel
		{
			ProjectId = Project.Id
		};

		return Page();
	}

	[BindProperty]
	public TemplateTbl CreateTemplate { get; set; } = new TemplateTbl();

	public async Task<IActionResult> OnPostCreateTemplate()
	{
		TemplateTbl result = await _templateTbl.Add(CreateTemplate).ConfigureAwait(false);

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(CreateTemplate.ProjectId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = $"Template created - {result.Name}";
		TempData["scrollToId"] = $"template-{result.Id}";

		return RedirectToPage("/project/details", new { id = CreateTemplate.ProjectId });
	}

	[BindProperty]
	public UpdateTemplateNameModel UpdateTemplateName { get; set; } = new UpdateTemplateNameModel();

	public async Task<IActionResult> OnPostUpdateTemplateName()
	{
		TemplateTbl? result = await _templateTbl.GetByID(UpdateTemplateName.TemplateId);

		if (result is null)
		{
			return NotFound();
		}

		if (UpdateTemplateName.ProjectId != result.ProjectId)
		{
			throw new ArgumentException(nameof(UpdateTemplateName.ProjectId));
		}

		result.Name = UpdateTemplateName.Name;
		await _templateTbl.Update(result);

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(UpdateTemplateName.ProjectId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = $"Template name update - {result.Name}";
		TempData["scrollToId"] = $"template-{result.Id}";

		return RedirectToPage("/Project/Details", new { id = UpdateTemplateName.ProjectId });
	}

	[BindProperty]
	public DeleteTemplateModel DeleteTemplate { get; set; } = new DeleteTemplateModel();

	public async Task<IActionResult> OnPostDeleteTemplate()
	{
		int? projectId = await _templateTbl
			.Where(x => x.Id.Equals(DeleteTemplate.TemplateId))
			.Select(x => x.ProjectId)
			.FirstOrDefaultAsync();

		if (projectId is null)
		{
			return NotFound();
		}

		if (DeleteTemplate.ProjectId != projectId)
		{
			throw new ArgumentException(nameof(DeleteTemplate.ProjectId));
		}

		await _templateTbl.Delete(DeleteTemplate.TemplateId);

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(DeleteTemplate.ProjectId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Template deleted";

		return RedirectToPage("/Project/Details", new { id = DeleteTemplate.ProjectId });
	}

	[BindProperty]
	public TemplateVersionTbl CreateTemplateVersion { get; set; } = new TemplateVersionTbl();

	public async Task<IActionResult> OnPostCreateTemplateVersion()
	{
		TemplateTbl? template = (await _templateTbl.Get(x => x.Id.Equals(CreateTemplateVersion.TemplateId), null, nameof(TemplateTbl.Versions))).FirstOrDefault();

		if (template is null)
		{
			return NotFound();
		}

		CreateTemplateVersion.Name = "Untitled";
		CreateTemplateVersion.Subject = "Default subject";
		CreateTemplateVersion.Html = string.Empty;
		CreateTemplateVersion.TestData = new List<TemplateTestDataTbl> {
			new TemplateTestDataTbl {
				Name = "Default",
				Data = "{}",
				IsDefault = true
			}
		};
		if (!template.Versions?.Any() ?? true)
		{
			CreateTemplateVersion.IsActive = true;
		}

		TemplateVersionTbl result = await _templateVersionTbl.Add(CreateTemplateVersion);

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(CreateTemplateVersion.Template.ProjectId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

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

		if (version is null)
		{
			return NotFound();
		}

		version.IsActive = true;

		await _templateVersionTbl.Update(version);

		await _templateVersionTbl.UpdateFromQuery(x =>
			x.IsActive &&
			!x.Id.Equals(version.Id) &&
			x.TemplateId.Equals(version.TemplateId),
			s => s
				.SetProperty(b => b.IsActive, _ => false));

		await _templateTbl.UpdateFromQuery(x => x.Id.Equals(MarkAsActive.TemplateId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(MarkAsActive.ProjectId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Template version marked as active";
		TempData["scrollToId"] = $"version-{version.Id}";

		return RedirectToPage("/Project/Details", new { id = MarkAsActive.ProjectId });
	}

	[BindProperty]
	public DuplicateTemplateVersionModel DuplicateTemplateVersion { get; set; } = new DuplicateTemplateVersionModel();

	public async Task<IActionResult> OnPostDuplicateTemplateVersion()
	{
		TemplateVersionTbl? version = (await _templateVersionTbl.Get(x =>
			   x.Id.Equals(DuplicateTemplateVersion.VersionId) &&
			   x.TemplateId.Equals(DuplicateTemplateVersion.TemplateId) &&
			   x.Template!.ProjectId.Equals(DuplicateTemplateVersion.ProjectId)))
		   .FirstOrDefault();

		if (version is null)
		{
			return NotFound();
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

		await _templateTbl.UpdateFromQuery(x => x.Id.Equals(DuplicateTemplateVersion.TemplateId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(DuplicateTemplateVersion.ProjectId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Template version duplicated";
		TempData["scrollToId"] = $"version-{result.Id}";

		return RedirectToPage("/Project/Details", new { id = DuplicateTemplateVersion.ProjectId });
	}

	[BindProperty]
	public DeleteTemplateVersionModel DeleteTemplateVersion { get; set; } = new DeleteTemplateVersionModel();

	public async Task<IActionResult> OnPostDeleteTemplateVersion()
	{
		TemplateVersionTbl? version = (await _templateVersionTbl.Get(x =>
				x.Id.Equals(DeleteTemplateVersion.VersionId) &&
				x.TemplateId.Equals(DeleteTemplateVersion.TemplateId) &&
				x.Template!.ProjectId.Equals(DeleteTemplateVersion.ProjectId)))
			.FirstOrDefault();

		if (version is null)
		{
			return NotFound();
		}

		await _templateVersionTbl.Delete(DeleteTemplateVersion.VersionId);

		await _templateTbl.UpdateFromQuery(x => x.Id.Equals(DeleteTemplateVersion.TemplateId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(DeleteTemplateVersion.ProjectId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

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

public class ProjectResponseModel
{
	public int Id { get; set; }
	public string Slug { get; set; } = default!;
	public string Name { get; init; } = default!;
	public string? SubHeading { get; set; }
	public string? Description { get; set; }
	public string? Tags { get; set; }
	public string ApiKey { get; init; } = default!;
	public List<TemplateResponseModel>? Templates { get; set; }
}

public class TemplateResponseModel
{
	public int Id { get; set; }
	public DateTime DateModified { get; set; }
	public string HashedApiId { get; set; } = null!;
	public string Name { get; set; } = string.Empty;

	public int ProjectId { get; set; }
	public List<TemplateVersionResponseModel>? Versions { get; set; }
}

public class TemplateVersionResponseModel
{
	public int Id { get; set; }
	public string HashedId { get; set; } = default!;
	public DateTime DateModified { get; set; }
	public string Name { get; set; } = string.Empty;
	public string TemplateNameSlug { get; set; } = string.Empty;
	public bool IsActive { get; set; }
	public string? ThumbnailImage { get; set; }
	public string? PreviewImage { get; set; }
}