using Common.CosmosDbServices.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Common.CosmosDbServices;

public abstract partial class CosmosDbService<T> : ICosmosDbService<T> where T : ICosmosAggregate
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

    public async Task<IEnumerable<T>> GetItemsAsync(string queryString, CancellationToken ct = default)
    {
        var query = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
        List<T> results = new();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(ct);

            results.AddRange(response.ToList());
        }

        return results;
    }

    public async Task<T?> GetAsync(string id, CancellationToken ct = default)
    {
        try
        {
            var result = await _container.ReadItemAsync<T>(id, new PartitionKey(id), cancellationToken: ct);
            if (result.StatusCode == System.Net.HttpStatusCode.OK) return result.Resource;
        }
        catch
        {

        }
        return default;
    }

    //changing the state of the application should be in the domain layer instead. It is here to demonstration purposes
    public async Task AddAsync(T item, CancellationToken cancellationToken = default)
    {
        //here we usually wanted to check the result and return it to the caller
        _ = await _container.CreateItemAsync(item, new PartitionKey(item.Id), cancellationToken: cancellationToken);
    }

    //changing the state of the application should be in the domain layer instead. It is here to demonstration purposes
    public async Task UpdateAsync(string id, T item, CancellationToken ct)
    {
        //this code is not production ready
        var result = await this._container.UpsertItemAsync<T>(item, new PartitionKey(id), cancellationToken: ct);
        var retries = 0;
        while (result.StatusCode == System.Net.HttpStatusCode.Conflict && retries++ < 3)
        {
            result = await this._container.UpsertItemAsync<T>(item, new PartitionKey(id), cancellationToken: ct);
        }
    }


}
