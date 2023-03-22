using Microsoft.Azure.Cosmos;

namespace Common.CosmosDbServices.Entities;

public interface ICosmosAggregate
{
    string Id { get; }
    PartitionKey PartitionKey { get; }
}
