using Database.Models;
using Database.Repositories.Generic;

namespace Database.Repositories.User;
public interface IUserRepository : IGenericRepository<UserTbl>
{
}
