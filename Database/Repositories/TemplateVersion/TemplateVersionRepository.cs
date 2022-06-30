using Database.Context;
using Database.Models;
using Database.Repositories.Generic;

namespace Database.Repositories.TemplateVersion;

public class TemplateVersionRepository : GenericRepository<TemplateVersionTbl>, ITemplateVersionRepository
{
	public TemplateVersionRepository(ApplicationDbContext context) : base(context)
	{
		base.context = context;
		dbSet = context.Set<TemplateVersionTbl>();
	}
}