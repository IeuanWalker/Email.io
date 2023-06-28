using Database.Context;
using Database.Models;
using Database.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories.Project;

public class ProjectRepository : GenericRepository<ProjectTbl>, IProjectRepository
{
	public ProjectRepository(ApplicationDbContext context) : base(context)
	{
		base.context = context;
		dbSet = context.Set<ProjectTbl>();
	}

	public async Task<List<string>> GetApiKeys()
	{
		return await dbSet.Select(x => x.ApiKey).ToListAsync();
	}
}