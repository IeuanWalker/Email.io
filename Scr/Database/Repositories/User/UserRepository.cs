using Database.Context;
using Database.Models;
using Database.Repositories.Generic;

namespace Database.Repositories.User;

public class UserRepository : GenericRepository<UserTbl>, IUserRepository
{
	public UserRepository(ApplicationDbContext context) : base(context)
	{
		base.context = context;
		dbSet = context.Set<UserTbl>();
	}
}