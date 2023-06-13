namespace Mtx.CosmosDbServices;

public record CosmosQuery(string Query)
{
	public static implicit operator QueryDefinition(CosmosQuery cosmosQuery) => new(cosmosQuery.Query);

}