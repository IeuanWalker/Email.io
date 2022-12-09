using Admin.Pages.Project;
using AutoMapper;
using Database.Models;

namespace Admin.Infrastructure.Maps;

public class TemplateProfile : Profile
{
	public TemplateProfile()
	{
		CreateMap<TemplateTbl, TemplateResponseModel>().ForMember(dest => dest.HashedApiId, act => act.Ignore());
		CreateMap<TemplateVersionTbl, TemplateVersionResponseModel>();
	}
}