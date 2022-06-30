using Database.Context;
using Database.Models;
using Database.Repositories.Generic;

namespace Database.Repositories.Project;

public class ProjectRepository : GenericRepository<ProjectTbl>, IProjectRepository
{
	public ProjectRepository(ApplicationDbContext context) : base(context)
	{
		base.context = context;
		dbSet = context.Set<ProjectTbl>();
	}
}