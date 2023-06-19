namespace Mtx.CosmosDbServices;

public interface ICosmosDbService
{
	Task<Result> AddAsync<T>(T item, PartitionKeyValue partitionKey,CancellationToken cancellationToken);
	Task<Result> AddUsingIdAsPartitionKeyAsync<T>(T item, CancellationToken cancellationToken);
	Task<DataResult<T>> GetAsync<T>(DocumentId id, PartitionKeyValue partitionKey, CancellationToken ct = default);
	Task<DataResult<T>> GetUsingIdAsPartitionKeyAsync<T>(DocumentId id, CancellationToken ct = default);
	Task<DataResult<List<T>>> GetItemsAsync<TQuery, T>(TQuery query, CancellationToken cancellationToken) where TQuery : CosmosQuery; 
	Task<DataResult<CountResult>> CountAsync<TQuery>(TQuery query, CancellationToken cancellationToken) where TQuery : CosmosQuery; 
	Task<Result> UpdateAsync<T>(T item, PartitionKeyValue partitionKey, CancellationToken ct);
	Task<Result> UpdateUsingIdAsPartitionKeyAsync<T>(T item, CancellationToken ct);
	Task<Result> DeleteAsync<T>(DocumentId id, PartitionKeyValue partitionKey, CancellationToken ct);
	Task<Result> DeleteUsingIdAsPartitionKeyAsync<T>(DocumentId id, CancellationToken ct);
	Task<Result> TransactionalBatchAddAsync<TContainerDefiningType,T>(IEnumerable<T> items, PartitionKeyValue partitionKey, CancellationToken cancellationToken);
}