using Database.Context;
using Database.Models;
using Database.Repositories.Generic;

namespace Database.Repositories.Email;

public class EmailRepository : GenericRepository<EmailTbl>, IEmailRepository
{
	public EmailRepository(ApplicationDbContext context) : base(context)
	{
		base.context = context;
		dbSet = context.Set<EmailTbl>();
	}
}