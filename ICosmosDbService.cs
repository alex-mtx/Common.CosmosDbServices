namespace Common.CosmosDbServices;

public interface ICosmosDbService
{
	Task AddAsync<T>(T item, object partitionKey,CancellationToken cancellationToken);
	Task<T?> GetAsync<T>(object id, CancellationToken ct = default);
	Task<List<T>> GetItemsAsync<T>(object query, CancellationToken cancellationToken);
	Task UpdateAsync<T>(T item, object id, object partitionKey, CancellationToken ct);
}