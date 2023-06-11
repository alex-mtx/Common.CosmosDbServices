namespace Mtx.CosmosDbServices.Entities;

public interface ICosmosDocumentIdentity
{
	string Id { get; }
	object PartitionKey { get; }
	string Container { get; }
}

