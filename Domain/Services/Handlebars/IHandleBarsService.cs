using System.Text.Json.Nodes;

namespace Domain.Services.Handlebars;

public interface IHandleBarsService
{
	/// <summary>
	/// Render a Handlebars template with data.
	/// </summary>
	/// <param name="template"></param>
	/// <param name="data"></param>
	string Render(string template, JsonNode data);
}