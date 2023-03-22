using Microsoft.Azure.Cosmos;

namespace Common.CosmosDbServices.Entities;
/// <summary>
/// Marks an entity as an Aggregate
/// </summary>
public interface ICosmosAggregate
{
    string Id { get; }
    PartitionKey PartitionKey { get; }
}
