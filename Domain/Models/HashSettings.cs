namespace Domain.Models;
public class HashSettings
{
	public HashSetting ProjectIdAndTemplateId { get; set; } = default!;
	public HashSetting ProjectId { get; set; } = default!;
	public HashSetting TemplateVersionId { get; set; } = default!;
	public HashSetting EmailId { get; set; } = default!;
}
public class HashSetting
{
	public string Salt { get; set; } = default!;
	public int MinLength { get; set; }
}