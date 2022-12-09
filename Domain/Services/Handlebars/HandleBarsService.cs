using System.Text.Json.Nodes;
using HandlebarsDotNet;

namespace Domain.Services.Handlebars;

public class HandlebarsService : IHandlebarsService
{
	public HandlebarsService()
	{
		RegisterHelpers();
	}

	public string Render(string template, JsonNode data)
	{
		// Compile template
		HandlebarsTemplate<object, object> compiledTemplate = HandlebarsDotNet.Handlebars.Compile(template);

		// Add data to template
		return compiledTemplate(data);
	}

	static void RegisterHelpers()
	{
		HandlebarsDotNet.Handlebars.RegisterHelper("ifCond", (output, options, context, arguments) =>
		{
			if (arguments.Length != 3)
			{
				throw new HandlebarsException("{{ifCond}} helper must have three arguments");
			}

			string v1 = arguments.At<string>(0);
			string @operator = arguments.At<string>(1);
			string v2 = arguments.At<string>(2);

			switch (@operator)
			{
				case "==":
					if (v1 == v2)
					{
						options.Template(output, context);
					}
					else
					{
						options.Inverse(output, context);
					}
					break;

				case "!=":
					if (v1 != v2)
					{
						options.Template(output, context);
					}
					else
					{
						options.Inverse(output, context);
					}
					break;

				case "<":
					if (Convert.ToDouble(v1) < Convert.ToDouble(v2))
					{
						options.Template(output, context);
					}
					else
					{
						options.Inverse(output, context);
					}
					break;

				case "<=":
					if (Convert.ToDouble(v1) <= Convert.ToDouble(v2))
					{
						options.Template(output, context);
					}
					else
					{
						options.Inverse(output, context);
					}
					break;

				case ">":
					if (Convert.ToDouble(v1) > Convert.ToDouble(v2))
					{
						options.Template(output, context);
					}
					else
					{
						options.Inverse(output, context);
					}
					break;

				case ">=":
					if (Convert.ToDouble(v1) >= Convert.ToDouble(v2))
					{
						options.Template(output, context);
					}
					else
					{
						options.Inverse(output, context);
					}
					break;
			}
		});
	}
}