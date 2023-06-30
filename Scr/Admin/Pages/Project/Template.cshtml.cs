using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Database.Models;
using Database.Repositories.Project;
using Database.Repositories.Template;
using Database.Repositories.TemplateTestData;
using Database.Repositories.TemplateVersion;
using Domain.Services.Email;
using Domain.Services.Handlebars;
using Domain.Services.HashId;
using Domain.Services.Slug;
using Domain.Services.Thumbnail;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;

namespace Admin.Pages.Project;

public class TemplateModel : PageModel
{
	readonly IProjectRepository _projectTbl;
	readonly ITemplateRepository _templateTbl;
	readonly ITemplateVersionRepository _templateVersionTbl;
	readonly ITemplateTestDataRepository _templateTestDataTbl;
	readonly IBackgroundJobClient _jobClient;
	readonly IEmailService _emailService;
	readonly IHashIdService _hashIdService;
	readonly ISlugService _slugService;
	readonly IHandlebarsService _handlebarsService;
	readonly IThumbnailService _thumbnailService;

	public TemplateModel(
		IProjectRepository projectTbl,
		ITemplateRepository templateTbl,
		ITemplateVersionRepository templateVersionTbl,
		ITemplateTestDataRepository templateTestDataTbl,
		IBackgroundJobClient jobClient,
		IEmailService emailService,
		IHashIdService hashIdService,
		ISlugService slugService,
		IHandlebarsService handlebarsService,
		IThumbnailService thumbnailService)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
		_templateTbl = templateTbl ?? throw new ArgumentNullException(nameof(templateTbl));
		_templateVersionTbl = templateVersionTbl ?? throw new ArgumentNullException(nameof(templateVersionTbl));
		_templateTestDataTbl = templateTestDataTbl ?? throw new ArgumentNullException(nameof(templateTestDataTbl));
		_jobClient = jobClient ?? throw new ArgumentNullException(nameof(jobClient));
		_emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
		_hashIdService = hashIdService ?? throw new ArgumentNullException(nameof(hashIdService));
		_slugService = slugService ?? throw new ArgumentNullException(nameof(slugService));
		_handlebarsService = handlebarsService ?? throw new ArgumentNullException(nameof(handlebarsService));
		_thumbnailService = thumbnailService ?? throw new ArgumentNullException(nameof(thumbnailService));
	}

	public TemplateVersionTbl Version { get; set; } = null!;
	public string ProjectSlug { get; set; } = default!;

	public async Task<IActionResult> OnGet(string slug, string hashedVersionId)
	{
		ProjectSlug = slug;

		int? projectId = _hashIdService.DecodeProjectId(_slugService.GetIdFromSlug(slug));
		int? versionId = _hashIdService.DecodeTemplateVersionId(hashedVersionId);

		if (projectId is null || versionId is null)
		{
			return NotFound();
		}

		// TODO: Pull minimal data
		TemplateVersionTbl? version = (await _templateVersionTbl.Get(x =>
				x.Id.Equals(versionId) &&
				x.Template!.ProjectId.Equals(projectId),
				includeProperties: nameof(TemplateVersionTbl.TestData)))
			.SingleOrDefault();

		if (version is null)
		{
			return NotFound();
		}

		Version = version;

		Version.TestData = Version.TestData.OrderByDescending(x => x.IsDefault).ToList();

		UpdateTemplate = new UpdateTemplateModel
		{
			ProjectId = (int)projectId,
			TemplateId = Version.TemplateId,
			VersionId = Version.Id
		};
		UpdateSettings = new UpdateSettingsModel
		{
			ProjectId = (int)projectId,
			TemplateId = Version.TemplateId,
			VersionId = Version.Id,
			Name = Version.Name,
			Subject = Version.Subject
		};
		TestSend = new TestSendModel
		{
			ProjectId = (int)projectId,
			TemplateId = Version.TemplateId,
			VersionId = Version.Id
		};

		AddTestData = new AddTestDataModel
		{
			ProjectSlug = slug,
			HashedTemplateVersionId = hashedVersionId
		};

		DuplicateTestData = MarkAsDefault = DeleteTestData = new DeleteTestDataModel
		{
			ProjectSlug = slug,
			HashedTemplateVersionId = hashedVersionId
		};

		UpdateTestDataName = new UpdateTestDataNameModel
		{
			ProjectSlug = slug,
			HashedTemplateVersionId = hashedVersionId
		};

		return Page();
	}

	[BindProperty]
	public UpdateTemplateModel UpdateTemplate { get; set; } = new UpdateTemplateModel();

	public async Task<JsonResult> OnPostUpdateTemplate([FromBody] UpdateTemplateModel UpdateTemplate)
	{
		UpdateTemplate.Html = string.IsNullOrWhiteSpace(UpdateTemplate.Html) ? string.Empty : UpdateTemplate.Html;
		UpdateTemplate.PlainText = string.IsNullOrWhiteSpace(UpdateTemplate.PlainText) ? string.Empty : UpdateTemplate.PlainText;

		// Validate JSON + template
		foreach (TemplateTestDataModel testData in UpdateTemplate.TestData)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(testData.Data))
				{
					testData.Data = "{}";
				}

				_handlebarsService.Render(UpdateTemplate.Html, testData.Data!);
				_handlebarsService.Render(UpdateTemplate.PlainText, testData.Data!);
			}
			catch (Exception)
			{
				return new JsonResult(new
				{
					toastStatus = "error",
					toastTitle = "Error parsing template failed.",
				});
			}
		}

		TemplateVersionTbl? version = (await _templateVersionTbl.Get(x =>
			  x.Id.Equals(UpdateTemplate.VersionId) &&
			  x.TemplateId.Equals(UpdateTemplate.TemplateId) &&
			  x.Template!.ProjectId.Equals(UpdateTemplate.ProjectId),
			  includeProperties: nameof(TemplateVersionTbl.TestData)))
		  .FirstOrDefault();

		if (version is null)
		{
			return new JsonResult(new
			{
				toastStatus = "error",
				toastTitle = "Error template not found.",
			});
		}

		version.Html = UpdateTemplate.Html;
		version.PlainText = UpdateTemplate.PlainText;
		foreach (TemplateTestDataModel testData in UpdateTemplate.TestData)
		{
			version.TestData.Where(x => x.Id.Equals(testData.Id)).ToList().ForEach(x => x.Data = testData.Data);
		}

		await _templateVersionTbl.Update(version);

		await _templateTbl.UpdateFromQuery(x => x.Id.Equals(UpdateTemplate.TemplateId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(UpdateTemplate.ProjectId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		_jobClient.Enqueue(() => _thumbnailService.GenerateThumbnail(version.Id));

		return new JsonResult(new
		{
			toastStatus = "success",
			toastTitle = "Template updated",
		});
	}

	[BindProperty]
	public UpdateSettingsModel UpdateSettings { get; set; } = new UpdateSettingsModel();

	public async Task<IActionResult> OnPostUpdateSettings()
	{
		TemplateVersionTbl? version = (await _templateVersionTbl.Get(x =>
		   x.Id.Equals(UpdateSettings.VersionId) &&
		   x.TemplateId.Equals(UpdateSettings.TemplateId) &&
		   x.Template!.ProjectId.Equals(UpdateSettings.ProjectId)))
	   .FirstOrDefault();

		if (version is null)
		{
			return NotFound();
		}

		version.Name = UpdateSettings.Name;
		version.Subject = UpdateSettings.Subject;

		await _templateVersionTbl.Update(version);

		await _templateTbl.UpdateFromQuery(x => x.Id.Equals(UpdateSettings.TemplateId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(UpdateSettings.ProjectId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Template updated";

		return RedirectToPage("/project/template", new
		{
			projectId = UpdateTemplate.ProjectId,
			templateId = UpdateTemplate.TemplateId,
			versionId = UpdateTemplate.VersionId
		});
	}

	[BindProperty]
	public TestSendModel TestSend { get; set; } = new TestSendModel();

	public async Task<IActionResult> OnPostTestSend()
	{
		// Get template
		TemplateVersionTbl? version = (await _templateVersionTbl.Get(x =>
				x.Id.Equals(TestSend.VersionId) &&
				x.TemplateId.Equals(TestSend.TemplateId) &&
				x.Template!.ProjectId.Equals(TestSend.ProjectId),
				includeProperties: nameof(TemplateVersionTbl.TestData)))
			.FirstOrDefault();

		if (version is null || version.Subject is null || version.Html is null)
		{
			return NotFound();
		}

		JsonNode? data = JsonNode.Parse(version.TestData.First()?.Data ?? string.Empty);
		if (data is null)
		{
			return NotFound();
		}

		// Generate body
		// TODO: Set test data to current view version
		string subjectResult = _handlebarsService.Render(version.Subject, data);
		string bodyResult = _handlebarsService.Render(version.Html, data);
		string plainTextResult = _handlebarsService.Render(version.PlainText ?? string.Empty, data);

		await _emailService.SendEmail(new List<MailboxAddress> { new MailboxAddress(TestSend.Name, TestSend.Email) }, null, null, subjectResult, bodyResult, plainTextResult);
		return RedirectToPage("/project/template", new
		{
			projectId = TestSend.ProjectId,
			templateId = TestSend.TemplateId,
			versionId = TestSend.VersionId
		});
	}

	[BindProperty]
	public AddTestDataModel AddTestData { get; set; } = new AddTestDataModel();

	public async Task<IActionResult> OnPostAddTestData()
	{
		int? templateVersionId = _hashIdService.DecodeTemplateVersionId(AddTestData.HashedTemplateVersionId);

		TemplateVersionTbl? templateVersion = (await _templateVersionTbl.Get(x => x.Id.Equals(templateVersionId))).FirstOrDefault();

		if (templateVersion is null)
		{
			return NotFound();
		}

		await _templateTestDataTbl.Add(new()
		{
			TemplateVersionId = templateVersion.Id,
			Name = "Untitled",
			Data = "{}"
		});

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "New test data created";

		return RedirectToPage("/project/template", new
		{
			slug = AddTestData.ProjectSlug,
			templateName = templateVersion.Name,
			hashedVersionId = AddTestData.HashedTemplateVersionId
		});
	}

	[BindProperty]
	public DeleteTestDataModel DeleteTestData { get; set; } = new DeleteTestDataModel();

	public async Task<IActionResult> OnPostDeleteTestData()
	{
		int? templateVersionId = _hashIdService.DecodeTemplateVersionId(DeleteTestData.HashedTemplateVersionId);

		TemplateTestDataTbl? testData = (await _templateTestDataTbl.Get(
			x => x.Id.Equals(DeleteTestData.TestDataId) && x.TemplateVersionId.Equals(templateVersionId),
			includeProperties: nameof(TemplateTestDataTbl.TemplateVersion))).FirstOrDefault();

		if (testData is null || testData.TemplateVersion is null)
		{
			return NotFound();
		}

		_templateTestDataTbl.Delete(testData);

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Test data deleted";

		return RedirectToPage("/project/template", new
		{
			slug = DeleteTestData.ProjectSlug,
			templateName = testData.TemplateVersion.Name,
			hashedVersionId = DeleteTestData.HashedTemplateVersionId
		});
	}

	[BindProperty]
	public DeleteTestDataModel MarkAsDefault { get; set; } = new DeleteTestDataModel();

	public async Task<IActionResult> OnPostMarkAsDefault()
	{
		int? templateVersionId = _hashIdService.DecodeTemplateVersionId(MarkAsDefault.HashedTemplateVersionId);

		TemplateTestDataTbl? testData = (await _templateTestDataTbl.Get(
			x => x.Id.Equals(MarkAsDefault.TestDataId) && x.TemplateVersionId.Equals(templateVersionId),
			includeProperties: nameof(TemplateTestDataTbl.TemplateVersion))).FirstOrDefault();

		if (testData is null || testData.TemplateVersion is null)
		{
			return NotFound();
		}

		testData.IsDefault = true;
		await _templateTestDataTbl.Update(testData);

		await _templateTestDataTbl.UpdateFromQuery(x =>
			x.IsDefault &&
			!x.Id.Equals(MarkAsDefault.TestDataId) &&
			x.TemplateVersionId.Equals(templateVersionId),
			s => s
				.SetProperty(b => b.IsDefault, _ => false));

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Test data set as default";

		return RedirectToPage("/project/template", new
		{
			slug = MarkAsDefault.ProjectSlug,
			templateName = testData.TemplateVersion.Name,
			hashedVersionId = MarkAsDefault.HashedTemplateVersionId
		});
	}

	[BindProperty]
	public DeleteTestDataModel DuplicateTestData { get; set; } = new DeleteTestDataModel();

	public async Task<IActionResult> OnPostDuplicateTestData()
	{
		int? templateVersionId = _hashIdService.DecodeTemplateVersionId(DuplicateTestData.HashedTemplateVersionId);

		TemplateTestDataTbl? testData = (await _templateTestDataTbl.Get(
			x => x.Id.Equals(DuplicateTestData.TestDataId) && x.TemplateVersionId.Equals(templateVersionId),
			includeProperties: nameof(TemplateTestDataTbl.TemplateVersion))).FirstOrDefault();

		if (testData is null || testData.TemplateVersion is null)
		{
			return NotFound();
		}

		await _templateTestDataTbl.Add(new TemplateTestDataTbl
		{
			Name = $"{testData.Name}_copy",
			Data = testData.Data,
			TemplateVersionId = testData.TemplateVersionId
		});

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Test data duplicated";

		return RedirectToPage("/project/template", new
		{
			slug = DuplicateTestData.ProjectSlug,
			templateName = testData.TemplateVersion.Name,
			hashedVersionId = DuplicateTestData.HashedTemplateVersionId
		});
	}

	[BindProperty]
	public UpdateTestDataNameModel UpdateTestDataName { get; set; } = new UpdateTestDataNameModel();

	public async Task<IActionResult> OnPostUpdateTestDataName()
	{
		int? templateVersionId = _hashIdService.DecodeTemplateVersionId(UpdateTestDataName.HashedTemplateVersionId);

		TemplateTestDataTbl? testData = (await _templateTestDataTbl.Get(
			x => x.Id.Equals(UpdateTestDataName.TestDataId) && x.TemplateVersionId.Equals(templateVersionId),
			includeProperties: nameof(TemplateTestDataTbl.TemplateVersion))).FirstOrDefault();

		if (testData is null || testData.TemplateVersion is null)
		{
			return NotFound();
		}

		testData.Name = UpdateTestDataName.Name;
		await _templateTestDataTbl.Update(testData);

		TempData["toastStatus"] = "success";
		TempData["toastMessage"] = "Test data name updated";

		return RedirectToPage("/project/template", new
		{
			slug = UpdateTestDataName.ProjectSlug,
			templateName = testData.TemplateVersion.Name,
			hashedVersionId = UpdateTestDataName.HashedTemplateVersionId
		});
	}
}

public class AddTestDataModel
{
	public string ProjectSlug { get; set; } = default!;
	public string HashedTemplateVersionId { get; set; } = default!;
}

public class DeleteTestDataModel
{
	public string ProjectSlug { get; set; } = default!;
	public string HashedTemplateVersionId { get; set; } = default!;
	public int TestDataId { get; set; }
}

public class UpdateTestDataNameModel
{
	public string ProjectSlug { get; set; } = default!;
	public string HashedTemplateVersionId { get; set; } = default!;
	public int TestDataId { get; set; }
	public string Name { get; set; } = default!;
}

public class UpdateTemplateModel
{
	[Required]
	public int ProjectId { get; set; }

	[Required]
	public int TemplateId { get; set; }

	[Required]
	public int VersionId { get; set; }

	[Required]
	public List<TemplateTestDataModel> TestData { get; set; } = new();

	[Required]
	public string Html { get; set; } = default!;

	[Required]
	public string PlainText { get; set; } = default!;
}

public class TemplateTestDataModel
{
	public int Id { get; set; }
	public string Data { get; set; } = default!;
}

public class UpdateSettingsModel
{
	[Required]
	public int ProjectId { get; set; }

	[Required]
	public int TemplateId { get; set; }

	[Required]
	public int VersionId { get; set; }

	[Required]
	[MaxLength(200)]
	public string Subject { get; set; } = default!;

	[Required]
	[MaxLength(200)]
	public string Name { get; set; } = default!;
}

public class TestSendModel
{
	[Required]
	public int ProjectId { get; set; }

	[Required]
	public int TemplateId { get; set; }

	[Required]
	public int VersionId { get; set; }

	public string? Name { get; set; }

	[Required]
	[EmailAddress]
	public string Email { get; set; } = default!;
}