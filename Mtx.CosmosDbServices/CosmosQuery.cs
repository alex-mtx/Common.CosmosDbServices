namespace Mtx.CosmosDbServices;

public record CosmosQuery
{
	private readonly QueryDefinition queryDefinition;

	public CosmosQuery(string query)
	{
		queryDefinition = new(query);
	}
	public void Add(params Param[] @params)
	{
		foreach (var param in @params)
		{

			queryDefinition.WithParameter(param.Name, param.Value);
		}

	}


	public static implicit operator QueryDefinition(CosmosQuery cosmosQuery) => cosmosQuery.queryDefinition;
}

public record CountResult(int Total)
{
	public bool Zero => Total == 0;
	public bool One => Total == 1;
	public bool Some => Total > 0;
}

public record LowerCaseText
{
	public LowerCaseText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			throw new ArgumentException($"'{nameof(text)}' cannot be null or empty.", nameof(text));
		}
		Value = text.ToLower();
	}

	public string Value { get; }
	public static implicit operator string(LowerCaseText text) => text.Value;

}

public record Param
{
	public Param(string name, string value)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
		}

		if (string.IsNullOrEmpty(value))
		{
			throw new ArgumentException($"'{nameof(value)}' cannot be null or empty.", nameof(value));
		}
		if (name[0] != '@') name = "@" + name;
		Name = name;
		Value = value;
	}

	public string Name { get; }
	public string Value { get; }
	public static Param Create(string paramName, string value) => new(paramName, value);
}