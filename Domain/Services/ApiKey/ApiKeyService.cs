using System.Security.Cryptography;
using Database.Repositories.Project;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Domain.Services.ApiKey;

public class ApiKeyService : IApiKeyService
{
	readonly IMemoryCache _memoryCache;
	readonly IProjectRepository _projectTbl;

	public ApiKeyService(IMemoryCache memoryCache, IProjectRepository projectTbl)
	{
		_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
		_projectTbl = projectTbl ?? throw new ArgumentNullException(nameof(projectTbl));
	}

	public async Task<string> GenerateUniqueApiKey()
	{
		while (true)
		{
			string apiKey = GenerateApiKey();
			if (!await _projectTbl.Where(x => x.ApiKey.Equals(apiKey)).AnyAsync())
			{
				return apiKey;
			}
		}
	}

	static string GenerateApiKey()
	{
		return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
			.Replace("/", "")
			.Replace("+", "")
			.Replace("=", "")
			[..36];
	}

	public async ValueTask<int?> GetProjectIdFromApiKey(string apiKey)
	{
		if (!_memoryCache.TryGetValue<Dictionary<string, int>>("Authentication_Project_ApiKeys", out var internalKeys))
		{
			internalKeys = await _projectTbl.GetAllApiKeysAndProjectIds();

			_memoryCache.Set("Authentication_Project_ApiKeys", internalKeys, DateTime.Now.AddHours(2));
		}

		return internalKeys is null || !internalKeys.TryGetValue(apiKey, out var projectId) ? null : projectId;
	}
}