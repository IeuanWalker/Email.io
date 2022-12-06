using System.ComponentModel.DataAnnotations;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using CoreHtmlToImage;
using Database.Models;
using Database.Repositories.Project;
using Database.Repositories.Template;
using Database.Repositories.TemplateTestData;
using Database.Repositories.TemplateVersion;
using Domain.Services.Email;
using Domain.Services.HashId;
using Domain.Services.Slug;
using HandlebarsDotNet;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;
using Newtonsoft.Json.Linq;

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

	public TemplateModel(
		IProjectRepository projectTbl,
		ITemplateRepository templateTbl,
		ITemplateVersionRepository templateVersionTbl,
		ITemplateTestDataRepository templateTestDataTbl,
		IBackgroundJobClient jobClient,
		IEmailService emailService,
		IHashIdService hashIdService,
		ISlugService slugService)
	{
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
		_templateTbl = templateTbl ?? throw new ArgumentNullException(nameof(templateTbl));
		_templateVersionTbl = templateVersionTbl ?? throw new ArgumentNullException(nameof(templateVersionTbl));
		_templateTestDataTbl = templateTestDataTbl ?? throw new ArgumentNullException(nameof(templateTestDataTbl));
		_jobClient = jobClient ?? throw new ArgumentNullException(nameof(jobClient));
		_emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
		_hashIdService = hashIdService ?? throw new ArgumentNullException(nameof(hashIdService));
		_slugService = slugService ?? throw new ArgumentNullException(nameof(slugService));
	}

	public TemplateVersionTbl? Version { get; set; }
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
		Version = (await _templateVersionTbl.Get(x =>
				x.Id.Equals(versionId) &&
				x.Template!.ProjectId.Equals(projectId),
				includeProperties: nameof(TemplateVersionTbl.TestData)))
			.FirstOrDefault();

		if (Version is null)
		{
			return NotFound();
		}

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
		HandlebarsTemplate<object, object> htmlTemplate = Handlebars.Compile(UpdateTemplate.Html);
		HandlebarsTemplate<object, object> plainTextTemplate = Handlebars.Compile(UpdateTemplate.PlainText);
		foreach (TemplateTestDataModel testData in UpdateTemplate.TestData)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(testData.Data))
				{
					testData.Data = "{}";
				}

				Handlebars.RegisterHelper("ifCond", (output, options, context, arguments) =>
				{
					if (arguments.Length != 3)
					{
						throw new HandlebarsException("{{#StringEqualityBlockHelper}} helper must have exactly two arguments");
					}

					string v1 = arguments.At<string>(0);
					string @operator = arguments.At<string>(1);
					string v2 = arguments.At<string>(0);

					switch (@operator)
					{
						case "==":
							if (v1 == v2)
							{
								options.Template(output, context);
							}
							else
							{
								options.Inverse(output, context);
							}
							break;

						case "!=":
							if (v1 != v2)
							{
								options.Template(output, context);
							}
							else
							{
								options.Inverse(output, context);
							}
							break;

						case "<":
							if (Convert.ToDouble(v1) < Convert.ToDouble(v2))
							{
								options.Template(output, context);
							}
							else
							{
								options.Inverse(output, context);
							}
							break;

						case "<=":
							if (Convert.ToDouble(v1) <= Convert.ToDouble(v2))
							{
								options.Template(output, context);
							}
							else
							{
								options.Inverse(output, context);
							}
							break;

						case ">":
							if (Convert.ToDouble(v1) > Convert.ToDouble(v2))
							{
								options.Template(output, context);
							}
							else
							{
								options.Inverse(output, context);
							}
							break;

						case ">=":
							if (Convert.ToDouble(v1) >= Convert.ToDouble(v2))
							{
								options.Template(output, context);
							}
							else
							{
								options.Inverse(output, context);
							}
							break;
					}
				});

				htmlTemplate(JObject.Parse(testData.Data));
				plainTextTemplate(JObject.Parse(testData.Data));
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

		_templateVersionTbl.Update(version);

		await _templateTbl.UpdateFromQuery(x => x.Id.Equals(UpdateTemplate.TemplateId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		await _projectTbl.UpdateFromQuery(x => x.Id.Equals(UpdateTemplate.ProjectId), s => s
			.SetProperty(b => b.DateModified, _ => DateTime.UtcNow));

		// TODO: Generate thumbnail
		_jobClient.Enqueue(() => GenerateThumbnailAndPreview(version.Id));

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

		_templateVersionTbl.Update(version);

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

		if (version is null)
		{
			return NotFound();
		}

		// Generate body
		Handlebars.RegisterHelper("ifCond", (output, options, context, arguments) =>
		   {
			   if (arguments.Length != 3)
			   {
				   throw new HandlebarsException("{{#StringEqualityBlockHelper}} helper must have exactly 3 arguments");
			   }

			   string v1 = arguments.At<string>(0);
			   string @operator = arguments.At<string>(1);
			   string v2 = arguments.At<string>(0);

			   switch (@operator)
			   {
				   case "==":
					   if (v1 == v2)
					   {
						   options.Template(output, context);
					   }
					   else
					   {
						   options.Inverse(output, context);
					   }
					   break;

				   case "!=":
					   if (v1 != v2)
					   {
						   options.Template(output, context);
					   }
					   else
					   {
						   options.Inverse(output, context);
					   }
					   break;

				   case "<":
					   if (Convert.ToDouble(v1) < Convert.ToDouble(v2))
					   {
						   options.Template(output, context);
					   }
					   else
					   {
						   options.Inverse(output, context);
					   }
					   break;

				   case "<=":
					   if (Convert.ToDouble(v1) <= Convert.ToDouble(v2))
					   {
						   options.Template(output, context);
					   }
					   else
					   {
						   options.Inverse(output, context);
					   }
					   break;

				   case ">":
					   if (Convert.ToDouble(v1) > Convert.ToDouble(v2))
					   {
						   options.Template(output, context);
					   }
					   else
					   {
						   options.Inverse(output, context);
					   }
					   break;

				   case ">=":
					   if (Convert.ToDouble(v1) >= Convert.ToDouble(v2))
					   {
						   options.Template(output, context);
					   }
					   else
					   {
						   options.Inverse(output, context);
					   }
					   break;
			   }
		   });

		// TODO: Set test data to current view version

		HandlebarsTemplate<object, object> subjectTemplate = Handlebars.Compile(version.Subject);
		string subjectResult = subjectTemplate(JObject.Parse(version.TestData.First().Data));

		HandlebarsTemplate<object, object> bodyTemplate = Handlebars.Compile(version.Html);
		string bodyResult = bodyTemplate(JObject.Parse(version.TestData.First().Data));

		HandlebarsTemplate<object, object> plainTextTemplate = Handlebars.Compile(version.PlainText);
		string plainTextResult = plainTextTemplate(JObject.Parse(version.TestData.First().Data));

		await _emailService.SendEmail(new List<MailboxAddress> { new MailboxAddress(TestSend.Name, TestSend.Email) }, null, null, subjectResult, bodyResult, plainTextResult);
		return RedirectToPage("/project/template", new
		{
			projectId = TestSend.ProjectId,
			templateId = TestSend.TemplateId,
			versionId = TestSend.VersionId
		});
	}

	public async Task GenerateThumbnailAndPreview(int versionId)
	{
		TemplateVersionTbl? version = (await _templateVersionTbl.Get(
			x => x.Id.Equals(versionId),
			null,
			nameof(TemplateVersionTbl.Template)))
			.FirstOrDefault();

		if (version is null)
		{
			return;
		}

		// Compile HTML and test data
		HandlebarsTemplate<object, object> template = Handlebars.Compile(version.Html);
		string result = template(JObject.Parse(version.TestData.First(x => x.IsDefault).Data));

		HtmlConverter converter = new();
		byte[] preview = converter.FromHtmlString(result, format: ImageFormat.Png);
		byte[] thumbnail = converter.FromHtmlString(result, 75, ImageFormat.Png, 50);

		Uri previewUri = await SaveImage(version.Template.ProjectId, preview, $"Template-{version.TemplateId}-Version-{version.Id}-preview.png");
		Uri thumbnailUri = await SaveImage(version.Template.ProjectId, thumbnail, $"Template-{version.TemplateId}-Version-{version.Id}-thumbnail.png");

		await _templateVersionTbl.UpdateFromQuery(x => x.Id.Equals(versionId), s => s
			.SetProperty(b => b.PreviewImage, _ => previewUri.ToString())
			.SetProperty(b => b.ThumbnailImage, _ => thumbnailUri.ToString()));
	}

	public async Task<Uri> SaveImage(int projectId, byte[] file, string name)
	{
		BlobContainerClient blobContainerClient = new(
			"UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite",
			$"project-{projectId.ToString().ToLower()}");
		await blobContainerClient.CreateIfNotExistsAsync();
		await blobContainerClient.SetAccessPolicyAsync(PublicAccessType.Blob);
		await blobContainerClient.DeleteBlobIfExistsAsync(name);

		Stream stream = new MemoryStream(file);
		await blobContainerClient.UploadBlobAsync(name, stream);

		BlobBaseClient client = new("UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite", $"project-{projectId.ToString().ToLower()}", name);

		return client.Uri.AbsoluteUri.Contains("azurite") ?
			new Uri(client.Uri.AbsoluteUri.Replace("azurite", "localhost")) :
			client.Uri;
	}

	[BindProperty]
	public AddTestDataModel AddTestData { get; set; } = new AddTestDataModel();

	public async Task<IActionResult> OnPostAddTestData()
	{
		var templateVersionId = _hashIdService.DecodeTemplateVersionId(AddTestData.HashedTemplateVersionId);

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
		var templateVersionId = _hashIdService.DecodeTemplateVersionId(DeleteTestData.HashedTemplateVersionId);

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
		var templateVersionId = _hashIdService.DecodeTemplateVersionId(MarkAsDefault.HashedTemplateVersionId);

		TemplateTestDataTbl? testData = (await _templateTestDataTbl.Get(
			x => x.Id.Equals(MarkAsDefault.TestDataId) && x.TemplateVersionId.Equals(templateVersionId),
			includeProperties: nameof(TemplateTestDataTbl.TemplateVersion))).FirstOrDefault();

		if (testData is null || testData.TemplateVersion is null)
		{
			return NotFound();
		}

		testData.IsDefault = true;
		_templateTestDataTbl.Update(testData);

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
		var templateVersionId = _hashIdService.DecodeTemplateVersionId(DuplicateTestData.HashedTemplateVersionId);

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
		var templateVersionId = _hashIdService.DecodeTemplateVersionId(UpdateTestDataName.HashedTemplateVersionId);

		TemplateTestDataTbl? testData = (await _templateTestDataTbl.Get(
			x => x.Id.Equals(UpdateTestDataName.TestDataId) && x.TemplateVersionId.Equals(templateVersionId),
			includeProperties: nameof(TemplateTestDataTbl.TemplateVersion))).FirstOrDefault();

		if (testData is null || testData.TemplateVersion is null)
		{
			return NotFound();
		}

		testData.Name = UpdateTestDataName.Name;
		_templateTestDataTbl.Update(testData);

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