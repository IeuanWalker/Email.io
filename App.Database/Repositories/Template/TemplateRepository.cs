using App.Database.Context;
using App.Database.Models;
using App.Database.Repositories.Generic;

namespace App.Database.Repositories.Template;

public class TemplateRepository : GenericRepository<TemplateTbl>, ITemplateRepository
{
	public TemplateRepository(ApplicationDbContext context) : base(context)
	{
		base.context = context;
		dbSet = context.Set<TemplateTbl>();
	}
}