
using Common.CosmosDbServices.Entities;

namespace Common.CosmosDbServices;

public interface ICosmosDbService<T> where T : ICosmosAggregate
{
	Task AddAsync(T item, CancellationToken cancellationToken = default);
	Task<T?> GetAsync(string id, CancellationToken ct = default);
	Task<IEnumerable<T>> GetItemsAsync(string queryString, CancellationToken cancellationToken = default);
	Task UpdateAsync(string id, T item, CancellationToken ct);
}