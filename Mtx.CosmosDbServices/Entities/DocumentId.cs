namespace Mtx.CosmosDbServices.Entities;

public record DocumentId
{
	public DocumentId(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentException($"'{nameof(id)}' cannot be null or empty.", nameof(id));
		}

		this.Id = id;
	}

	public string Id { get; }

	public static DocumentId From<T>(T source)
	{
		if (default(T) is null && source?.ToString() is null)
			throw new ArgumentException($"'{nameof(source)}' cannot be null or empty.", nameof(Id));
		return new DocumentId(source.ToString());
	}

	public PartitionKeyValue ToPartitionKey() => PartitionKeyValue.From(Id);

	public static implicit operator string(DocumentId docId) => docId.Id;
}
