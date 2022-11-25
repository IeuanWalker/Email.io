using HashidsNet;

namespace Domain.Services.HashId;
public class HashIdService : IHashIdService
{
	const string _salt = "my-salt";
	public string EncodeProjectAndTemplateId(int projectId, int templateId)
	{
		return new Hashids(_salt, 30).Encode(projectId, templateId);
	}

	public (int projectId, int templateId) DecodeProjectAndTemplateId(string hash)
	{
		var test=  new Hashids(_salt).Decode(hash);

		return (test[0], test[1]);
	}

	public string Encode(int id, int minLength = 10)
	{
		return new Hashids(_salt, minLength).Encode(id);
	}

	public int? Decode(string hash)
	{
		try
		{
			return new Hashids(_salt, 10).DecodeSingle(hash);
		} 
		catch(Exception)
		{
			return null;
		}
	}
}
