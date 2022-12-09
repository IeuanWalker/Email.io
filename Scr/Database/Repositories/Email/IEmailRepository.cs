using Database.Models;
using Database.Repositories.Generic;

namespace Database.Repositories.Email;

public interface IEmailRepository : IGenericRepository<EmailTbl>
{
}