namespace Mtx.CosmosDbServices.Entities;

public record CosmosDocumentIdentity(string Id, object PartitionKey, string Container) : ICosmosDocumentIdentity
{
	public static CosmosDocumentIdentity From(string id, object PartitionKey, string Container) => new CosmosDocumentIdentity(id, PartitionKey, Container);
}


