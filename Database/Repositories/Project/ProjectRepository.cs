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

	public async Task<Dictionary<string, int>> GetAllApiKeysAndProjectIds()
	{
		return await dbSet.ToDictionaryAsync(x => x.ApiKey, x => x.Id);
	}
}