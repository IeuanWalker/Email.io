using App.Database.Context;
using App.Database.Models;
using App.Database.Repositories.Generic;

namespace App.Database.Repositories.Project;

public class ProjectRepository : GenericRepository<ProjectTbl>, IProjectRepository
{
	public ProjectRepository(ApplicationDbContext context) : base(context)
	{
		base.context = context;
		dbSet = context.Set<ProjectTbl>();
	}
}