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

	// TODO: Benchmark
	//static string GenerateApiKey()
	//{
	//	using var rng = new RNGCryptoServiceProvider();
	//	var apiKeyBytes = new byte[32];
	//	rng.GetBytes(apiKeyBytes);
	//	var apiKey = Convert.ToBase64String(apiKeyBytes);

	//	// Clean up the API key by removing certain characters
	//	apiKey = apiKey.Replace("/", "").Replace("+", "").Replace("=", "");

	//	// Return the first 36 characters of the API key
	//	return apiKey.Substring(0, 36);
	//}

	public async ValueTask<bool> DoesApiKeyExist(string apiKey)
	{
		if (!_memoryCache.TryGetValue("ApiKeys", out List<string>? apiKeys))
		{
			apiKeys = await _projectTbl.GetApiKeys();

			_memoryCache.Set("ApiKeys", apiKeys, DateTime.Now.AddHours(2));
		}

		return apiKeys?.Contains(apiKey) ?? false;
	}
}