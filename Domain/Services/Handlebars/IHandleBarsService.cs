using System.Text.Json.Nodes;

namespace Domain.Services.Handlebars;

public interface IHandleBarsService
{
	string Render(string template, JsonNode data);
}