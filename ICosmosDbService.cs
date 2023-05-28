namespace Mtx.CosmosDbServices;

public interface ICosmosDbService
{
	Task<Result> AddAsync<T>(T item, object id, object partitionKey,CancellationToken cancellationToken);
	Task<Result> AddAsync<T>(T item, ICosmosDocumentIdentity identity, CancellationToken cancellationToken);
	Task<DataResult<T>> GetAsync<T>(object id, object partitionKey, CancellationToken ct = default);
	Task<DataResult<List<T>>> GetItemsAsync<T>(object query, CancellationToken cancellationToken);
	Task<Result> UpdateAsync<T>(T item, object id, object partitionKey, CancellationToken ct);
	Task<Result> UpdateAsync<T>(T item, ICosmosDocumentIdentity identity, CancellationToken ct);
}