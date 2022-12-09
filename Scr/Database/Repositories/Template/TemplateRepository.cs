using Database.Context;
using Database.Models;
using Database.Repositories.Generic;

namespace Database.Repositories.Template;

public class TemplateRepository : GenericRepository<TemplateTbl>, ITemplateRepository
{
	public TemplateRepository(ApplicationDbContext context) : base(context)
	{
		base.context = context;
		dbSet = context.Set<TemplateTbl>();
	}
}