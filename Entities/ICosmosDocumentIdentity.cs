using Microsoft.Azure.Cosmos;
using System.Text.Json.Serialization;

namespace Common.CosmosDbServices.Entities;

public interface ICosmosDocumentIdentity
{
    [JsonIgnore]
    string CosmosId { get; }
    [JsonIgnore]
	PartitionKey PartitionKey { get; }
}


