using HashidsNet;

namespace Domain.Services.HashId;

public class HashIdService : IHashIdService
{
	const string salt = "my-salt";

	public string EncodeProjectAndTemplateId(int projectId, int templateId)
	{
		return new Hashids(salt, 30).Encode(projectId, templateId);
	}

	public (int projectId, int templateId)? DecodeProjectAndTemplateId(string hash)
	{
		try
		{
			var result = new Hashids(salt, 30).Decode(hash);

			return (result[0], result[1]);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public string Encode(int id, int minLength = 10)
	{
		return new Hashids(salt, minLength).Encode(id);
	}

	public int? Decode(string hash, int minLength = 10)
	{
		try
		{
			return new Hashids(salt, minLength).DecodeSingle(hash);
		}
		catch (Exception)
		{
			return null;
		}
	}
}