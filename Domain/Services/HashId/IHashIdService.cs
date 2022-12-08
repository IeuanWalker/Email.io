namespace Domain.Services.HashId;

public interface IHashIdService
{
	/// <summary>
	/// Encodes the given project and template IDs into a hash.
	/// </summary>
	/// <param name="projectId"></param>
	/// <param name="templateId"></param>
	string EncodeProjectAndTemplateId(int projectId, int templateId);

	/// <summary>
	/// Decodes hash into project and template Id
	/// </summary>
	/// <param name="hash"></param>
	(int projectId, int templateId)? DecodeProjectAndTemplateId(string hash);

	/// <summary>
	/// Encodes the given project ID into a hash.
	/// </summary>
	/// <param name="projectId"></param>
	string EncodeProjectId(int projectId);

	/// <summary>
	/// Decodes hash into project Id
	/// </summary>
	/// <param name="hash"></param>
	int? DecodeProjectId(string hash);

	/// <summary>
	/// Encodes the given TemplateVersion ID into a hash.
	/// </summary>
	/// <param name="templateVersionId"></param>
	string EncodeTemplateVersionId(int templateVersionId);

	/// <summary>
	/// Decodes hash into TemplateVersion Id
	/// </summary>
	/// <param name="hash"></param>
	int? DecodeTemplateVersionId(string hash);

	/// <summary>
	/// Encodes the given email ID into a hash.
	/// </summary>
	/// <param name="emailId"></param>
	string EncodeEmailId(int emailId);

	/// <summary>
	/// Decodes hash into email Id
	/// </summary>
	/// <param name="hash"></param>
	int? DecodeEmailId(string hash);
}