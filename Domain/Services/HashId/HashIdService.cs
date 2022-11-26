using HashidsNet;

namespace Domain.Services.HashId;

public class HashIdService : IHashIdService
{
	const string _salt = "my-salt";

	public string EncodeProjectAndTemplateId(int projectId, int templateId)
	{
		return new Hashids(_salt, 30).Encode(projectId, templateId);
	}

	public (int projectId, int templateId)? DecodeProjectAndTemplateId(string hash)
	{
		try
		{
			var test = new Hashids(_salt, 30).Decode(hash);

			return (test[0], test[1]);
		}
		catch (Exception ex)
		{
			return null;
		}
	}

	public string Encode(int id, int minLength = 10)
	{
		return new Hashids(_salt, minLength).Encode(id);
	}

	public int? Decode(string hash, int minLength = 10)
	{
		try
		{
			return new Hashids(_salt, minLength).DecodeSingle(hash);
		}
		catch (Exception)
		{
			return null;
		}
	}
}