using Domain.Models;
using HashidsNet;
using Microsoft.Extensions.Options;

namespace Domain.Services.HashId;

public class HashIdService : IHashIdService
{
	readonly Hashids _hashidsProjectIdAndTemplateId;
	readonly Hashids _hashidsProjectId;
	readonly Hashids _hashidsTemplateVersionId;
	readonly Hashids _hashidsEmailId;

	public HashIdService(IOptions<HashSettings> hashSettings)
	{
		_hashidsProjectIdAndTemplateId = new(hashSettings.Value.ProjectIdAndTemplateId.Salt, hashSettings.Value.ProjectIdAndTemplateId.MinLength);
		_hashidsProjectId = new(hashSettings.Value.ProjectId.Salt, hashSettings.Value.ProjectId.MinLength);
		_hashidsTemplateVersionId = new(hashSettings.Value.TemplateVersionId.Salt, hashSettings.Value.TemplateVersionId.MinLength);
		_hashidsEmailId = new(hashSettings.Value.EmailId.Salt, hashSettings.Value.EmailId.MinLength);
	}

	public string EncodeProjectAndTemplateId(int projectId, int templateId)
	{
		return _hashidsProjectIdAndTemplateId.Encode(projectId, templateId);
	}

	public (int projectId, int templateId)? DecodeProjectAndTemplateId(string hash)
	{
		try
		{
			int[]? result = _hashidsProjectIdAndTemplateId.Decode(hash);
			return result is null || result.Length is not 2 ?
				null :
				(result[0], result[1]);
		}
		catch (ArgumentOutOfRangeException)
		{
			return null;
		}
		catch (OverflowException)
		{
			return null;
		}
	}

	public string EncodeProjectId(int projectId)
	{
		return _hashidsProjectId.Encode(projectId);
	}

	public int? DecodeProjectId(string hash)
	{
		return Decode(_hashidsProjectId, hash);
	}

	public string EncodeTemplateVersionId(int templateVersionId)
	{
		return _hashidsTemplateVersionId.Encode(templateVersionId);
	}

	public int? DecodeTemplateVersionId(string hash)
	{
		return Decode(_hashidsTemplateVersionId, hash);
	}

	public string EncodeEmailId(int emailId)
	{
		return _hashidsEmailId.Encode(emailId);
	}

	public int? DecodeEmailId(string hash)
	{
		return Decode(_hashidsEmailId, hash);
	}

	static int? Decode(Hashids hashids, string hash)
	{
		try
		{
			return hashids.TryDecodeSingle(hash, out int result) ? result : null;
		}
		catch (OverflowException)
		{
			return null;
		}
	}
}