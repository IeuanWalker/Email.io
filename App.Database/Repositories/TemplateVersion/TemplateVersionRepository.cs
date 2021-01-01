using App.Database.Context;
using App.Database.Models;
using App.Database.Repositories.Generic;

namespace App.Database.Repositories.TemplateVersion
{
    public class TemplateVersionRepository : GenericRepository<TemplateVersionTbl>, ITemplateVersionRepository
    {
        public TemplateVersionRepository(ApplicationDbContext context) : base(context)
        {
            Context = context;
            DbSet = context.Set<TemplateVersionTbl>();
        }
    }
}