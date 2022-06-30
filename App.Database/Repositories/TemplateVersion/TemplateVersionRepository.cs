using App.Database.Context;
using App.Database.Models;
using App.Database.Repositories.Generic;

namespace App.Database.Repositories.TemplateVersion;

public class TemplateVersionRepository : GenericRepository<TemplateVersionTbl>, ITemplateVersionRepository
{
	public TemplateVersionRepository(ApplicationDbContext context) : base(context)
	{
		base.context = context;
		dbSet = context.Set<TemplateVersionTbl>();
	}
}