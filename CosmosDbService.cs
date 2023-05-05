using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Common.CosmosDbServices;

public abstract class CosmosDbService : ICosmosDbService
{
	protected abstract string ContainerName { get; }
	protected abstract string DatabaseName { get; }
	protected readonly Container _container;

	public CosmosDbService(
		CosmosClient dbClient,
		IOptions<CosmosDbOptions> options)
	{
		_container = dbClient.GetContainer(DatabaseName, ContainerName);
	}

	public async Task<List<T>> GetItemsAsync<T>(object query, CancellationToken ct)
	{
		var queryIterator = _container.GetItemQueryIterator<T>(new QueryDefinition(query.ToString()));
		List<T> results = new();
		while (queryIterator.HasMoreResults)
		{
			var response = await queryIterator.ReadNextAsync(ct);

			results.AddRange(response.ToList());
		}

		return results;
	}

	public async Task<T?> GetAsync<T>(object id, CancellationToken ct)
	{
		try
		{
			var result = await _container.ReadItemAsync<T>(id.ToString(), new PartitionKey(id.ToString()), cancellationToken: ct);
			if (result.StatusCode == System.Net.HttpStatusCode.OK) return result.Resource;
		}
		catch
		{

		}
		return default;
	}

	//changing the state of the application should be in the domain layer instead. It is here to demonstration purposes
	public async Task AddAsync<T>(T item, object partitionKey, CancellationToken cancellationToken)
	{
		//here we usually wanted to check the result and return it to the caller
		_ = await _container.CreateItemAsync(item, new PartitionKey(partitionKey.ToString()), cancellationToken: cancellationToken);
	}

	//changing the state of the application should be in the domain layer instead. It is here to demonstration purposes
	public async Task UpdateAsync<T>(T item, object id, object partitionKey, CancellationToken ct)
	{
		//this code is not production ready
		var result = await this._container.UpsertItemAsync<T>(item, new PartitionKey(partitionKey.ToString()), cancellationToken: ct);
		var retries = 0;
		while (result.StatusCode == System.Net.HttpStatusCode.Conflict && retries++ < 3)
		{
			result = await this._container.UpsertItemAsync<T>(item, new PartitionKey(partitionKey.ToString()), cancellationToken: ct);
		}
	}

}
