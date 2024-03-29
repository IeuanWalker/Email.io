using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using App.Database.Models;
using App.Database.Repositories.Project;
using App.Database.Repositories.Template;
using App.Database.Repositories.TemplateVersion;
using App.Services.Email;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using CoreHtmlToImage;
using HandlebarsDotNet;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;
using Newtonsoft.Json.Linq;

namespace App.Pages.Project
{
    public class TemplateModel : PageModel
    {
        private readonly IProjectRepository _projectTbl;
        private readonly ITemplateRepository _templateTbl;
        private readonly ITemplateVersionRepository _templateVersionTbl;
        private IBackgroundJobClient _jobClient;
        private readonly IEmailService _emailService;
        public TemplateModel(
            IProjectRepository projectTbl,
            ITemplateRepository templateTbl,
            ITemplateVersionRepository templateVersionTbl,
            IBackgroundJobClient jobClient,
            IEmailService emailService)
        {
            _projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
            _templateTbl = templateTbl ?? throw new ArgumentNullException(nameof(templateTbl));
            _templateVersionTbl = templateVersionTbl ?? throw new ArgumentNullException(nameof(templateVersionTbl));
            _jobClient = jobClient ?? throw new ArgumentNullException(nameof(jobClient));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public TemplateVersionTbl Version { get; set; }
        public Guid ProjectId { get; set; }

        public async Task OnGet(Guid projectId, Guid templateId, int versionId)
        {
            ProjectId = projectId;
            // TODO: Error handling
            Version = (await _templateVersionTbl.Get(x =>
                    x.Id.Equals(versionId) &&
                    x.TemplateId.Equals(templateId) &&
                    x.Template.ProjectId.Equals(projectId)))
                .FirstOrDefault();

            if (Version == null)
                throw new NullReferenceException();

            UpdateTemplate = new UpdateTemplateModel
            {
                ProjectId = projectId,
                TemplateId = templateId,
                VersionId = versionId
            };
            UpdateSettings = new UpdateSettingsModel
            {
                ProjectId = projectId,
                TemplateId = templateId,
                VersionId = versionId,
                Name = Version.Name,
                Subject = Version.Subject
            };
            TestSend = new TestSendModel
            {
                ProjectId = projectId,
                TemplateId = templateId,
                VersionId = versionId
            };
        }

        [BindProperty]
        public UpdateTemplateModel UpdateTemplate { get; set; }
        public async Task<IActionResult> OnPostUpdateTemplate()
        {
            UpdateTemplate.Html = string.IsNullOrWhiteSpace(UpdateTemplate.Html) ? string.Empty : UpdateTemplate.Html;
            UpdateTemplate.TestData = string.IsNullOrWhiteSpace(UpdateTemplate.TestData) ? "{}" : UpdateTemplate.TestData;
            try
            {
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
                HandlebarsTemplate<object, object> template = Handlebars.Compile(UpdateTemplate.Html);
                template(JObject.Parse(UpdateTemplate.TestData));
            }
            catch (Exception ex)
            {
                TempData["toastStatus"] = "error";
                TempData["toastMessage"] = "Parsing template failed. Please check ";

                return RedirectToPage("/Project/Template", new
                {
                    projectId = UpdateTemplate.ProjectId,
                    templateId = UpdateTemplate.TemplateId,
                    versionId = UpdateTemplate.VersionId
                });
            }


            // TODO: Error handling
            TemplateVersionTbl version = (await _templateVersionTbl.Get(x =>
                  x.Id.Equals(UpdateTemplate.VersionId) &&
                  x.TemplateId.Equals(UpdateTemplate.TemplateId) &&
                  x.Template.ProjectId.Equals(UpdateTemplate.ProjectId)))
              .FirstOrDefault();

            if (version == null)
                throw new NullReferenceException();

            version.TestData = UpdateTemplate.TestData;
            version.Html = UpdateTemplate.Html;

            _templateVersionTbl.Update(version);

            await _templateTbl.UpdateFromQuery(x => x.Id.Equals(UpdateTemplate.TemplateId), _ => new TemplateTbl
            {
                DateModified = DateTime.Now
            });
            await _projectTbl.UpdateFromQuery(x => x.Id.Equals(UpdateTemplate.ProjectId), _ => new ProjectTbl
            {
                DateModified = DateTime.Now
            });

            // TODO: Generate thumbnail
            _jobClient.Enqueue(() => GenerateThumbnailAndPreview(version.Id));



            TempData["toastStatus"] = "success";
            TempData["toastMessage"] = "Template updated";

            return RedirectToPage("/Project/Template", new
            {
                projectId = UpdateTemplate.ProjectId,
                templateId = UpdateTemplate.TemplateId,
                versionId = UpdateTemplate.VersionId
            });
        }

        [BindProperty]
        public UpdateSettingsModel UpdateSettings { get; set; }
        public async Task<IActionResult> OnPostUpdateSettings()
        {
            TemplateVersionTbl version = (await _templateVersionTbl.Get(x =>
               x.Id.Equals(UpdateSettings.VersionId) &&
               x.TemplateId.Equals(UpdateSettings.TemplateId) &&
               x.Template.ProjectId.Equals(UpdateSettings.ProjectId)))
           .FirstOrDefault();

            if (version == null)
                throw new NullReferenceException();

            version.Name = UpdateSettings.Name;
            version.Subject = UpdateSettings.Subject;

            _templateVersionTbl.Update(version);


            await _templateTbl.UpdateFromQuery(x => x.Id.Equals(UpdateSettings.TemplateId), _ => new TemplateTbl
            {
                DateModified = DateTime.Now
            });
            await _projectTbl.UpdateFromQuery(x => x.Id.Equals(UpdateSettings.ProjectId), _ => new ProjectTbl
            {
                DateModified = DateTime.Now
            });

            TempData["toastStatus"] = "success";
            TempData["toastMessage"] = "Template updated";

            return RedirectToPage("/Project/Template", new
            {
                projectId = UpdateTemplate.ProjectId,
                templateId = UpdateTemplate.TemplateId,
                versionId = UpdateTemplate.VersionId
            });
        }
        [BindProperty]
        public TestSendModel TestSend { get; set; }
        public async Task<IActionResult> OnPostTestSend()
        {
            // Get template 
            TemplateVersionTbl version = (await _templateVersionTbl.Get(x =>
                    x.Id.Equals(UpdateSettings.VersionId) &&
                    x.TemplateId.Equals(UpdateSettings.TemplateId) &&
                    x.Template.ProjectId.Equals(UpdateSettings.ProjectId)))
                .FirstOrDefault();

            if (version == null)
                throw new NullReferenceException();

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
            HandlebarsTemplate<object, object> subjectTemplate = Handlebars.Compile(version.Subject);
            string subjectResult = subjectTemplate(JObject.Parse(version.TestData));

            HandlebarsTemplate<object, object> bodyTemplate = Handlebars.Compile(version.Html);
            string bodyResult = bodyTemplate(JObject.Parse(version.TestData));

            await _emailService.SendEmail(new List<MailboxAddress> { new MailboxAddress(TestSend.Name, TestSend.Email) }, subjectResult, bodyResult, version.TestData);
            return RedirectToPage("/Project/Template", new
            {
                projectId = TestSend.ProjectId,
                templateId = TestSend.TemplateId,
                versionId = TestSend.VersionId
            });
        }

        public async Task GenerateThumbnailAndPreview(int versionId)
        {
            TemplateVersionTbl version = (await _templateVersionTbl.Get(
                x => x.Id.Equals(versionId),
                null,
                nameof(TemplateVersionTbl.Template)))
                .FirstOrDefault();

            // Compile HTML and test data
            HandlebarsTemplate<object, object> template = Handlebars.Compile(version.Html);
            string result = template(JObject.Parse(version.TestData));

            //
            HtmlConverter converter = new HtmlConverter();
            byte[] preview = converter.FromHtmlString(result, format: ImageFormat.Png);
            byte[] thumbnail = converter.FromHtmlString(result, 75, ImageFormat.Png, 50);

            Uri previewUri = await SaveImage(version.Template.ProjectId, preview, $"Template-{version.TemplateId}-Version-{version.Id}-preview.png");
            Uri thumbnailUri = await SaveImage(version.Template.ProjectId, thumbnail, $"Template-{version.TemplateId}-Version-{version.Id}-thumbnail.png");

            await _templateVersionTbl.UpdateFromQuery(x => x.Id.Equals(versionId), _ => new TemplateVersionTbl
            {
                PreviewImage = previewUri.ToString(),
                ThumbnailImage = thumbnailUri.ToString()
            });
        }


        public async Task<Uri> SaveImage(Guid projectId, byte[] file, string name)
        {

            BlobContainerClient blobContainerClient = new BlobContainerClient(
                "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;",
                $"project-{projectId.ToString().ToLower()}");
            await blobContainerClient.CreateIfNotExistsAsync();
            await blobContainerClient.SetAccessPolicyAsync(PublicAccessType.Blob);
            await blobContainerClient.DeleteBlobIfExistsAsync(name);

            Stream stream = new MemoryStream(file);
            await blobContainerClient.UploadBlobAsync(name, stream);

            BlobBaseClient client = new BlobBaseClient("AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;", $"project-{projectId.ToString().ToLower()}", name);
            return client.Uri;

        }
    }

    public class UpdateTemplateModel
    {
        [Required]
        public Guid ProjectId { get; set; }
        [Required]
        public Guid TemplateId { get; set; }
        [Required]
        public int VersionId { get; set; }
        [Required]
        public string TestData { get; set; }
        [Required]
        public string Html { get; set; }
    }

    public class UpdateSettingsModel
    {
        [Required]
        public Guid ProjectId { get; set; }
        [Required]
        public Guid TemplateId { get; set; }
        [Required]
        public int VersionId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Subject { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
    }

    public class TestSendModel
    {
        [Required]
        public Guid ProjectId { get; set; }
        [Required]
        public Guid TemplateId { get; set; }
        [Required]
        public int VersionId { get; set; }

        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
