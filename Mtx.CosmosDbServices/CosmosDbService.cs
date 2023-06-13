using Microsoft.Extensions.Options;
using Mtx.CosmosDbServices.Extensions;
using System.Net;

namespace Mtx.CosmosDbServices;

public class CosmosDbService2 : ICosmosDbService
{

	protected readonly CosmosClient _dbClient;

	protected virtual string DatabaseName { get; }
	public static EntityContainerMap IdentityMap { get; set; } = new EntityContainerMap();
	public CosmosDbService2(
		CosmosClient dbClient,
		IOptions<CosmosDbOptions> options)
	{
		if (dbClient is null)
		{
			throw new ArgumentNullException(nameof(dbClient));
		}
		_dbClient = dbClient;

		if (options is null || options.Value is null)
		{
			throw new ArgumentNullException(nameof(options));
		}
		DatabaseName = options.Value.DbName;

	}
	protected DocumentId GetIdFrom<T>(T item)
	{
		dynamic? looseItem = item;
		if (looseItem is null || looseItem.GetType().GetProperty("Id") == null)
			throw new ArgumentException("must have Id as property and must not be null", nameof(item));

		return DocumentId.From(looseItem.Id);

	}

	public async Task<Result> AddAsync<T>(T item, PartitionKeyValue partitionKey, CancellationToken cancellationToken)
	{
		try
		{
			var container = GetContainerFor<T>();
			var response = await container.CreateItemAsync(item, partitionKey, cancellationToken: cancellationToken);
			return response.ToResult();
		}
		catch (Exception e)
		{

			return Result.InternalErrorWithGenericErrorMessage(exception: e);
		}
	}

	public async Task<Result> AddUsingIdAsPartitionKeyAsync<T>(T item, CancellationToken cancellationToken)
	{
		var id = GetIdFrom(item);
		var partitionKey = id.ToPartitionKey();
		return await AddAsync(item,partitionKey,cancellationToken);
	}

	public async Task<Result> DeleteAsync<T>(DocumentId id, PartitionKeyValue partitionKey, CancellationToken ct)
	{
		var container = GetContainerFor<T>();
		var response =  await container.DeleteItemAsync<T>(id,partitionKey, cancellationToken: ct);
		return response.ToResult();
	}

	public async Task<Result> DeleteUsingIdAsPartitionKeyAsync<T>(DocumentId id, CancellationToken ct)
	{
		return await DeleteAsync<T>(id, id.ToPartitionKey(), ct);
	}

	public async Task<DataResult<T>> GetAsync<T>(DocumentId id, PartitionKeyValue partitionKey, CancellationToken cancellationToken = default)
	{
		try
		{
			var container = GetContainerFor<T>();

			var result = await container.ReadItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken);
			if (result.StatusCode == HttpStatusCode.OK) return DataResult<T>.Ok200(result.Resource);
			if (result.StatusCode == HttpStatusCode.NotFound) return DataResult<T>.NotFound404();
			return DataResult<T>.InternalError("A unknown internal error occurred");
		}
		catch (Exception e)
		{
			return DataResult<T>.InternalError(error: e.Message, exception: e);
		}
	}

	public async Task<DataResult<T>> GetUsingIdAsPartitionKeyAsync<T>(DocumentId id, CancellationToken ct = default)
	{
		return await GetAsync<T>(id,id.ToPartitionKey(),cancellationToken: ct);
	}

	public async Task<DataResult<List<T>>> GetItemsAsync<T>(CosmosQuery query, CancellationToken cancellationToken)
	{
		var container = GetContainerFor<T>();

		var queryIterator = container.GetItemQueryIterator<T>(query);
		if (!queryIterator.HasMoreResults)
			DataResult<List<T>>.NoContent204();

		List <T> results = new();
		while (queryIterator.HasMoreResults)
		{
			var response = await queryIterator.ReadNextAsync(cancellationToken);

			results.AddRange(response.ToList());
		}

		return DataResult<List<T>>.Ok200(results);
	}

	public async Task<Result> UpdateAsync<T>(T item, PartitionKeyValue partitionKey, CancellationToken ct)
	{
		try
		{
			var container = GetContainerFor<T>();

			var response = await container.UpsertItemAsync<T>(item, partitionKey, cancellationToken: ct);
			return response.ToResult();
		}
		catch (Exception e)
		{
			return Result.InternalError(error: e.Message, exception: e);

		}
	}

	public async Task<Result> UpdateUsingIdAsPartitionKeyAsync<T>(T item, CancellationToken ct)
	{
		var id = GetIdFrom(item);
		return await UpdateAsync(item, id.ToPartitionKey(), ct);	
	}

	protected Container GetContainerFor<T>()
	{
		var containerName = IdentityMap.GetForType<T>();
		var container = _dbClient.GetContainer(DatabaseName, containerName);
		return container;
	}

	public Task<Result> UpdateUsingIdAsPartitionKeyAsync<T>(T item, CancellationToken ct)
	{
		throw new NotImplementedException();
	}
}

public class CosmosDbService : ICosmosDbService
{
	protected readonly CosmosClient _dbClient;

	protected virtual string DatabaseName { get; }
	public static EntityContainerMap IdentityMap { get; set; } = new EntityContainerMap();
	public CosmosDbService(
		CosmosClient dbClient,
		IOptions<CosmosDbOptions> options)
	{
		if (dbClient is null)
		{
			throw new ArgumentNullException(nameof(dbClient));
		}
		_dbClient = dbClient;

		if (options is null || options.Value is null)
		{
			throw new ArgumentNullException(nameof(options));
		}
		DatabaseName = options.Value.DbName;

	}
	protected (string Id, PartitionKey PartitionKey, Container Contaier) GetFromMap<T>(object id, object partitionKey)
	{
		var containerName = IdentityMap.GetForType<T>();
		var container = _dbClient.GetContainer(DatabaseName, containerName);
		var key = new PartitionKey(partitionKey.ToString());
		return new(id.ToString(), key, container);

	}

	protected (string Id, PartitionKey PartitionKey, Container Contaier) GetFromMap<T>(T item)
	{
		dynamic looseItem = item;
		if (looseItem is null || looseItem.GetType().GetProperty("Id") == null)
			throw new ArgumentException("must have Id as property and must not be null", nameof(item));

		var container = GetContainerFor<T>();

		var key = new PartitionKey(looseItem.Id.ToString());
		return new(looseItem.Id.ToString(), key, container);

	}

	protected (string Id, string PartitionKey) GetFromInstanceOf<T>(T item)
	{
		dynamic looseItem = item;
		if (looseItem is null || looseItem.GetType().GetProperty("Id") == null)
			throw new ArgumentException("must have Id as property and must not be null", nameof(item));
		return new(looseItem.Id.ToString(), looseItem.Id.ToString());
	}

	protected string GetIdFrom<T>(T item)
	{
		dynamic looseItem = item;
		if (looseItem is null || looseItem.GetType().GetProperty("Id") == null || looseItem.Id.ToString() is null)
			throw new ArgumentException("must have Id as property and must not be null", nameof(item));
		return looseItem.Id.ToString();
	}

	protected PartitionKey GetPartitionKeyFromIdOf<T>(T item)
	{
		return new PartitionKey(GetIdFrom(item));
	}

	protected (string Id, PartitionKey PartitionKey, Container Contaier) GetFromMap<T>(object id)
	{
		var container = GetContainerFor<T>();

		var key = new PartitionKey(id.ToString());
		return new(id.ToString(), key, container);

	}

	protected Container GetContainerFor<T>()
	{
		var containerName = IdentityMap.GetForType<T>();
		var container = _dbClient.GetContainer(DatabaseName, containerName);
		return container;
	}
	public async Task<Result> AddUsingIdAsPartitionKeyAsync<T>(T item, CancellationToken cancellationToken)
	{
		try
		{
			var container = GetContainerFor<T>();
			var key = GetPartitionKeyFromIdOf(item);
			var response = await container.CreateItemAsync(item, key, cancellationToken: cancellationToken);
			return new Result((int)response.StatusCode);
		}
		catch (Exception e)
		{

			return Result.InternalError(error: "A unknown internal error occurred", exception: e);
		}
	}

	public async Task<DataResult<T>> GetAsync<T>(object id, CancellationToken ct = default)
	{
		return await GetAsync<T>(id, id, ct);
	}
	public async Task<DataResult<T>> GetAsync<T>(object id, object partitionKey, CancellationToken ct = default)
	{
		try
		{
			var (idToQuery, key, container) = GetFromMap<T>(id);

			var result = await container.ReadItemAsync<T>(idToQuery, key, cancellationToken: ct);
			if (result.StatusCode == HttpStatusCode.OK) return DataResult<T>.Ok200(result.Resource);
			if (result.StatusCode == HttpStatusCode.NotFound) return DataResult<T>.NotFound404();
			return DataResult<T>.InternalError("A unknown internal error occurred");
		}
		catch (Exception e)
		{
			return DataResult<T>.InternalError(error: e.Message, exception: e);
		}
	}

	public async Task<DataResult<List<T>>> GetItemsAsync<T>(object query, CancellationToken cancellationToken)
	{
		var container = GetContainerFor<T>();

		var queryIterator = container.GetItemQueryIterator<T>(new QueryDefinition(query.ToString()));
		List<T> results = new();
		while (queryIterator.HasMoreResults)
		{
			var response = await queryIterator.ReadNextAsync(cancellationToken);

			results.AddRange(response.ToList());
		}

		return new(StatusCodes.Status200OK, Contents: results);
	}

	public async Task<Result> UpdateAsync<T>(T item, object partitionKey, CancellationToken ct)
	{
		try
		{
			var container = GetContainerFor<T>();

			var result = await container.UpsertItemAsync<T>(item, new PartitionKey(partitionKey.ToString()), cancellationToken: ct);
			return new Result((int)result.StatusCode);
		}
		catch (Exception e)
		{
			return Result.InternalError(error: e.Message, exception: e);

		}
	}

}
