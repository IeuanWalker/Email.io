namespace Domain.Services.HashId;

public interface IHashIdService
{
	string EncodeProjectAndTemplateId(int projectId, int templateId);
	(int projectId, int templateId)? DecodeProjectAndTemplateId(string hash);
	string EncodeProjectId(int projectId);
	int? DecodeProjectId(string hash);
	string EncodeTemplateVersionId(int templateVersionId);
	int? DecodeTemplateVersionId(string hash);
	string EncodeEmailId(int emailId);
	int? DecodeEmailId(string hash);

}