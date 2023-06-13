namespace Mtx.CosmosDbServices.Entities;

public record PartitionKeyValue
{
	public PartitionKeyValue(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));
		}

		this.Value = key;
	}

	public string Value { get; }

	public static PartitionKeyValue From<T>(T source)
	{
		if (default(T) is null && source?.ToString() is null)
			throw new ArgumentException($"'{nameof(Value)}' cannot be null or empty.", nameof(Value));
		return new PartitionKeyValue(source.ToString());
	}

	public static implicit operator PartitionKey(PartitionKeyValue partitionKeyValue) =>	new (partitionKeyValue.Value);
}