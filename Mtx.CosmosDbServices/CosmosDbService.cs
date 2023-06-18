using Microsoft.Extensions.Logging;
using Mtx.CosmosDbServices.Extensions;
using System.Net;

namespace Mtx.CosmosDbServices;

internal class CosmosDbService : ICosmosDbService
{

	public readonly IContainerFactory _containerFactory;
	private readonly ILogger<CosmosDbService> logger;

	public CosmosDbService(IContainerFactory containerFactory, ILogger<CosmosDbService> logger)
	{
		_containerFactory = containerFactory ?? throw new ArgumentNullException(nameof(containerFactory));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	protected static DocumentId GetIdFrom<T>(T item)
	{
		dynamic? looseItem = item;
		if (looseItem is null || looseItem.GetType().GetProperty("Id") == null)
			throw new ArgumentException("must have Id as property and must not be null", nameof(item));

		return DocumentId.From(looseItem.Id);

	}

	private Container GetContainerFor<T>() => _containerFactory.CreateFor<T>();

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
			logger.LogError(exception: e, "Could not add item");
			return Result.InternalErrorWithGenericErrorMessage(exception: e);
		}
	}

	public async Task<Result> AddUsingIdAsPartitionKeyAsync<T>(T item, CancellationToken cancellationToken)
	{
		var id = GetIdFrom(item);
		var partitionKey = id.ToPartitionKey();
		return await AddAsync(item, partitionKey, cancellationToken);
	}

	public async Task<Result> DeleteAsync<T>(DocumentId id, PartitionKeyValue partitionKey, CancellationToken ct)
	{
		var container = GetContainerFor<T>();
		var response = await container.DeleteItemAsync<T>(id, partitionKey, cancellationToken: ct);
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
			logger.LogError(exception: e, "Could not fetch item");

			return DataResult<T>.InternalError(error: e.Message, exception: e);
		}
	}

	public async Task<DataResult<T>> GetUsingIdAsPartitionKeyAsync<T>(DocumentId id, CancellationToken ct = default)
	{
		return await GetAsync<T>(id, id.ToPartitionKey(), cancellationToken: ct);
	}

	public async Task<DataResult<List<T>>> GetItemsAsync<T>(CosmosQuery query, CancellationToken cancellationToken)
	{
		try
		{
			var container = GetContainerFor<T>();
			List<T> results = new();

			using (var queryIterator = container.GetItemQueryIterator<T>(query))
			{
				if (!queryIterator.HasMoreResults)
					DataResult<List<T>>.NoContent204();

				while (queryIterator.HasMoreResults)
				{
					var response = await queryIterator.ReadNextAsync(cancellationToken);

					results.AddRange(response.ToList());
				}
			}

			return DataResult<List<T>>.Ok200(results);
		}
		catch (Exception e)
		{

			logger.LogError(exception: e, "Could not fetch items");

			return DataResult<List<T>>.InternalError(error: e.Message, exception: e);
		}
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
			logger.LogError(exception: e, "Could not update item");
			return Result.InternalError(error: e.Message, exception: e);

		}
	}

	public async Task<Result> UpdateUsingIdAsPartitionKeyAsync<T>(T item, CancellationToken ct)
	{
		var id = GetIdFrom(item);
		return await UpdateAsync(item, id.ToPartitionKey(), ct);
	}



}