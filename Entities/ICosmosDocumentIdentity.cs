using Microsoft.Azure.Cosmos;
using System.Text.Json.Serialization;

namespace Mtx.CosmosDbServices.Entities;

public interface ICosmosDocumentIdentity
{
    [JsonIgnore]
    string CosmosId { get; }
    [JsonIgnore]
	PartitionKey PartitionKey { get; }
}


