namespace Domain.Services.HashId;

public interface IHashIdService
{
	string Encode(int id, int minLength = 10);

	int? Decode(string hash, int minLength = 10);

	string EncodeProjectAndTemplateId(int projectId, int templateId);

	(int projectId, int templateId)? DecodeProjectAndTemplateId(string hash);
}