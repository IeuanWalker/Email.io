using App.Database.Context;
using App.Database.Models;
using App.Database.Repositories.Generic;

namespace App.Database.Repositories.Template
{
    public class TemplateRepository : GenericRepository<TemplateTbl>, ITemplateRepository
    {
        public TemplateRepository(ApplicationDbContext context) : base(context)
        {
            Context = context;
            DbSet = context.Set<TemplateTbl>();
        }
    }
}