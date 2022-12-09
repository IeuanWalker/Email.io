namespace Domain.Services.ApiKey;

/// <summary>
/// Thanks to this article for the implementation - https://www.camiloterevinto.com/post/simple-and-secure-api-keys-using-asp-net-core
/// </summary>
public interface IApiKeyService
{
	Task<string> GenerateUniqueApiKey();

	ValueTask<int?> GetProjectIdFromApiKey(string apiKey);
}