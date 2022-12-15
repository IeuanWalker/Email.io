using System.Text.Json.Nodes;
using Domain.Services.Handlebars;
using HandlebarsDotNet;
using NSubstitute.ExceptionExtensions;

namespace UnitTests.Domain.Services;

/// <summary>
/// Tests a custom Handlebars helper and the built in helpers (https://handlebarsjs.com/guide/builtin-helpers.html#lookup)
/// </summary>
public class HandlebarsService_Tests
{
	readonly IHandlebarsService _sut;

	public HandlebarsService_Tests()
	{
		_sut = new HandlebarsService();
	}

	[Fact]
	public void TestRender_WithValidTemplateAndData_ShouldRenderTemplate()
	{
		// Arrange
		const string template = "Hello {{name}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "name", "Ieuan" }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello Ieuan");
	}

	[Fact]
	public void TestRender_WithInvalidTemplateAndData_ShouldThrowException()
	{
		// Arrange
		const string template = "Hello {{#ifCond name '==' }}Ieuan{{else}}other person{{/ifCond}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "name", "Ieuan" }
		});

		// Act
		Action act = () => _sut.Render(template, data);

		// Assert
		act.Should().Throw<HandlebarsException>();
	}

	//! Test for custom helpers

	[Fact]
	public void TestRender_WithIfCondHelperAndEqualValues_ShouldRenderTemplate()
	{
		// Arrange
		const string template = "Hello {{#ifCond name '==' 'Ieuan'}}Ieuan{{else}}other person{{/ifCond}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "name", "Ieuan" }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello Ieuan");
	}

	[Fact]
	public void TestRender_WithIfCondHelperAndNonEqualValues_ShouldRenderInverse()
	{
		// Arrange
		const string template = "Hello {{#ifCond name '==' 'Ryan'}}Ryan{{else}}other person{{/ifCond}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "name", "Ieuan" }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello other person");
	}

	[Fact]
	public void TestRender_WithIfCondHelperAndNonEqualOperator_ShouldRenderTemplate()
	{
		// Arrange
		const string template = "Hello {{#ifCond name '!=' 'Jane'}}Ieuan{{else}}Jane{{/ifCond}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "name", "Ieuan" }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello Ieuan");
	}

	[Fact]
	public void TestRender_WithIfCondHelperAndNonEqualOperator_ShouldRenderInverse()
	{
		// Arrange
		const string template = "Hello {{#ifCond name '!=' 'Ieuan'}}John{{else}}Ieuan{{/ifCond}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "name", "Ieuan" }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello Ieuan");
	}

	[Fact]
	public void TestRender_WithIfCondHelperAndLessThanValues_ShouldRenderTemplate()
	{
		// Arrange
		const string template = "Hello {{#ifCond age '<' '20'}}teenager{{else}}adult{{/ifCond}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "age", 15 }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello teenager");
	}

	[Fact]
	public void TestRender_WithIfCondHelperAndLessThanValues_ShouldRenderInverse()
	{
		// Arrange
		const string template = "Hello {{#ifCond age '<' '20'}}teenager{{else}}adult{{/ifCond}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "age", 25 }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello adult");
	}

	[Fact]
	public void TestRender_WithIfCondHelperAndLessThanOrEqualValues_ShouldRenderTemplate()
	{
		// Arrange
		const string template = "Hello {{#ifCond age '<=' '20'}}teenager or adult{{else}}adult{{/ifCond}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "age", 20 }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello teenager or adult");
	}

	[Fact]
	public void TestRender_WithIfCondHelperAndLessThanOrEqualValues_ShouldRenderInverse()
	{
		// Arrange
		const string template = "Hello {{#ifCond age '<=' '20'}}teenager or adult{{else}}adult{{/ifCond}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "age", 25 }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello adult");
	}

	[Fact]
	public void TestRender_WithIfCondHelperAndGreaterThanValues_ShouldRenderTemplate()
	{
		// Arrange
		const string template = "Hello {{#ifCond age '>' '20'}}adult{{else}}teenager or adult{{/ifCond}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "age", 25 }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello adult");
	}

	[Fact]
	public void TestRender_WithIfCondHelperAndGreaterThanValues_ShouldRenderInverse()
	{
		// Arrange
		const string template = "Hello {{#ifCond age '>' '20'}}adult{{else}}teenager or adult{{/ifCond}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "age", 15 }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello teenager or adult");
	}

	[Fact]
	public void TestRender_WithIfCondHelperAndGreaterThanOrEqualValues_ShouldRenderTemplate()
	{
		// Arrange
		const string template = "Hello {{#ifCond age '>=' '20'}}adult{{else}}teenager or adult{{/ifCond}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "age", 20 }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello adult");
	}

	[Fact]
	public void TestRender_WithIfCondHelperAndGreaterThanOrEqualValues_ShouldRenderInverse()
	{
		// Arrange
		const string template = "Hello {{#ifCond age '>=' '20'}}adult{{else}}teenager or adult{{/ifCond}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "age", 15 }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello teenager or adult");
	}

	//! Test for standard handlebars helpers

	[Fact]
	public void TestRender_WithIfHelperAndTruthyValue_ShouldRenderTemplate()
	{
		// Arrange
		const string template = "Hello {{#if name}}{{name}}{{else}}world{{/if}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "name", "Ieuan" }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello Ieuan");
	}

	[Fact]
	public void TestRender_WithIfHelperAndFalsyValue_ShouldRenderInverse()
	{
		// Arrange
		const string template = "Hello {{#if name}}{{name}}{{else}}world{{/if}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "name", null }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello world");
	}

	[Fact]
	public void TestRender_WithUnlessHelperAndFalsyValue_ShouldRenderTemplate()
	{
		// Arrange
		const string template = "Hello {{#unless name}}world{{else}}{{name}}{{/unless}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "name", null }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello world");
	}

	[Fact]
	public void TestRender_WithUnlessHelperAndTruthyValue_ShouldRenderInverse()
	{
		// Arrange
		const string template = "Hello {{#unless name}}world{{else}}{{name}}{{/unless}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "name", "Ieuan" }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello Ieuan");
	}

	[Fact]
	public void TestRender_WithEachHelper_ShouldRenderTemplate()
	{
		// Arrange
		const string template = "Hello {{#each people}}{{this}}{{/each}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "people", new JsonArray("John", "Jane", "Ieuan") }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello JohnJaneIeuan");
	}

	[Fact]
	public void TestRender_WithEachHelperAndEmptyArray_ShouldRenderInverse()
	{
		// Arrange
		const string template = "Hello {{#each people}}{{this}}{{else}}world{{/each}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "people", null }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello world");
	}

	[Fact]
	public void TestRender_WithWithHelper_ShouldRenderTemplate()
	{
		// Arrange
		const string template = "Hello {{#with user}}{{name}}{{/with}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{
				"user", new JsonObject(new Dictionary<string, JsonNode?>
				{
					{ "name", "Ieuan" }
				})
			}
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello Ieuan");
	}

	[Fact]
	public void TestRender_WithWithHelperAndFalsyValue_ShouldRenderInverse()
	{
		// Arrange
		const string template = "Hello {{#with user}}{{name}}{{else}}world{{/with}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{ "user", null }
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Hello world");
	}

	[Fact]
	public void TestRender_WithLookupHelper_ShouldRenderTemplate1()
	{
		// Arrange
		const string template = "{{#each people}}{{.}} lives in {{lookup ../cities @index}}{{/each}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{
				"people",  new JsonArray("Nils", "Yehuda")
			},
			{
				"cities",  new JsonArray("Darmstadt", "San Francisco")
			}
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		result.Should().Be("Nils lives in DarmstadtYehuda lives in San Francisco");
	}

	//! Large complex test
	[Fact]
	public void TestRender_WithHugeTemplateAndData_ShouldRenderTemplate()
	{
		// Arrange
		const string template =
	@"{{#with user}}
{{#unless name}}
Hi there!
{{else}}
Hello {{name}}!
{{#ifCond age '>=' '18'}}You are an adult.{{else}}You are a {{#ifCond age '>=' '13'}}teenager{{else}}child{{/ifCond}}.{{/ifCond}}
{{/unless}}
{{#each hobbies}}
- {{this}}
{{/each}}
{{#ifCond country '==' 'United Kingdom'}}
You're from the UK!
{{else}}
Where are you from?
{{/ifCond}}
{{/with}}
{{#each items}}
{{#ifCond this '==' 'apple'}}
An apple a day keeps the doctor away.
{{else}}
This is a {{this}}.
{{/ifCond}}{{/each}}";
		JsonObject data = new(new Dictionary<string, JsonNode?>
		{
			{
				"user", new JsonObject(new Dictionary<string, JsonNode?>
				{
					{ "name", "Ieuan" },
					{ "age", 26 },
					{
						"hobbies", new JsonArray("programming", "gaming", "music")
					},
					{ "country", "United Kingdom" }
				})
			},
			{
				"items", new JsonArray("apple", "banana", "pear")
			}
		});

		// Act
		string result = _sut.Render(template, data);

		// Assert
		const string expectedResult =
	@"Hello Ieuan!
You are an adult.
- programming
- gaming
- music
You're from the UK!
An apple a day keeps the doctor away.
This is a banana.
This is a pear.
";
		result.Should().Be(expectedResult);
	}
}