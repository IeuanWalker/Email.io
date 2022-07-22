namespace EmailApi.Models;

public class EmailModel
{
	public List<string> ToAddresses { get; set; } = new List<string>();
	public string Subject { get; set; } = string.Empty;
	public string Body { get; set; } = string.Empty;
}
