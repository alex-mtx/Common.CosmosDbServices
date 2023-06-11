using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;

namespace Mtx.CosmosDbServices;

public class CosmosDbService : ICosmosDbService
{
	private readonly CosmosClient _dbClient;

	protected virtual string DatabaseName { get; }
	public static IdentityMap IdentityMap { get; set; } = new IdentityMap();
	public CosmosDbService(
		CosmosClient dbClient,
		IOptions<CosmosDbOptions> options)
	{
		if (dbClient is null)
		{
			throw new ArgumentNullException(nameof(dbClient));
		}
		

		if (options is null || options.Value is null)
		{
			throw new ArgumentNullException(nameof(options));
		}

	}

	public async Task<Result> AddAsync<T>(T item, object id, object partitionKey, CancellationToken cancellationToken)
	{
		try
		{
		var _container = _dbClient.GetContainer(options.Value.DbName, ContainerName);

			var container = _dbClient.GetContainer(options.Value.DbName, ContainerName);

			var response = await _container.CreateItemAsync(item, new PartitionKey(partitionKey.ToString()), cancellationToken: cancellationToken);
			return new Result((int)response.StatusCode);
		}
		catch (Exception e)
		{

			return Result.InternalError(error: e.Message, exception: e);
		}
	}

	public async Task<Result> AddAsync<T>(T item, ICosmosDocumentIdentity identity, CancellationToken cancellationToken)
	{

		return await AddAsync(item, identity.Id, identity.PartitionKey, cancellationToken: cancellationToken);
	}

	public async Task<DataResult<T>> GetAsync<T>(object id, object partitionKey, CancellationToken ct = default)
	{
		try
		{
			var result = await _container.ReadItemAsync<T>(id.ToString(), new PartitionKey(id.ToString()), cancellationToken: ct);
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
		var queryIterator = _container.GetItemQueryIterator<T>(new QueryDefinition(query.ToString()));
		List<T> results = new();
		while (queryIterator.HasMoreResults)
		{
			var response = await queryIterator.ReadNextAsync(cancellationToken);

			results.AddRange(response.ToList());
		}

		return new(StatusCodes.Status200OK, Contents: results);
	}

	public async Task<Result> UpdateAsync<T>(T item, object id, object partitionKey, CancellationToken ct)
	{
		try
		{
			var result = await this._container.UpsertItemAsync<T>(item, new PartitionKey(partitionKey.ToString()), cancellationToken: ct);
			return new Result((int)result.StatusCode);
		}
		catch (Exception e)
		{
			return Result.InternalError(error: e.Message, exception: e);

		}
	}

	public async Task<Result> UpdateAsync<T>(T item, ICosmosDocumentIdentity identity, CancellationToken ct)
	{
		return await UpdateAsync(item, identity.Id, identity.PartitionKey, ct);
	}
}

public class IdentityMap
{

	private Dictionary<Type, dynamic> mappings = new Dictionary<Type, dynamic>();
	public void AddFor<T>(Func<T, ICosmosDocumentIdentity> func) where T : class
	{
		mappings[typeof(T)] = func;
	}
	public ICosmosDocumentIdentity GetFor<T>(T entity) where T : class
	{
		return mappings[typeof(T)].Invoke(entity);
	}

}
