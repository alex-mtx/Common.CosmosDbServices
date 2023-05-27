using Mtx.CosmosDbServices.Entities;

namespace Mtx.CosmosDbServices;

public interface ICosmosDbService
{
	Task AddAsync<T>(T item, object id, object partitionKey,CancellationToken cancellationToken);
	Task AddAsync<T>(T item,CancellationToken cancellationToken) where T : ICosmosDocumentIdentity;
	Task<T?> GetAsync<T>(object id, CancellationToken ct = default);
	Task<List<T>> GetItemsAsync<T>(object query, CancellationToken cancellationToken);
	Task UpdateAsync<T>(T item, object id, object partitionKey, CancellationToken ct);
}