using Database.Context;
using Database.Models;
using Database.Repositories.Generic;

namespace Database.Repositories.TemplateTestData;

public class TemplateTestDataRepository : GenericRepository<TemplateTestDataTbl>, ITemplateTestDataRepository
{
	public TemplateTestDataRepository(ApplicationDbContext context) : base(context)
	{
		base.context = context;
		dbSet = context.Set<TemplateTestDataTbl>();
	}
}