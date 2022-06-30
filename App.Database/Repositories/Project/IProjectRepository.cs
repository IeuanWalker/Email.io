using App.Database.Models;
using App.Database.Repositories.Generic;

namespace App.Database.Repositories.Project;

public interface IProjectRepository : IGenericRepository<ProjectTbl>
{
}