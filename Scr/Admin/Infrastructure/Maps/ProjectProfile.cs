using Admin.Pages.Project;
using AutoMapper;
using Database.Models;

namespace Admin.Infrastructure.Maps;

public class ProjectProfile : Profile
{
	public ProjectProfile()
	{
		CreateMap<ProjectTbl, ProjectResponseModel>();
		CreateMap<ProjectTbl, ProjectResponseModel1>();
	}
}