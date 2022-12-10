using Database.Models;
using Database.Repositories.Generic;

namespace Database.Repositories.Project;

public interface IProjectRepository : IGenericRepository<ProjectTbl>
{
	Task<bool> DoesApiKeyExist(string apiKey);
}