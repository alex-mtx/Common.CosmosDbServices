using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Mtx.CosmosDbServices;

public class ContainerFactory : IContainerFactory
{
	protected readonly CosmosClient _dbClient;

	protected virtual string DatabaseName { get; }
	public static EntityContainerMap IdentityMap { get; set; } = new EntityContainerMap();
	public ContainerFactory(
		CosmosClient dbClient,
		IOptions<CosmosDbOptions> options)
	{
		if (dbClient is null)
		{
			throw new ArgumentNullException(nameof(dbClient));
		}
		_dbClient = dbClient;

		if (options is null || options.Value is null)
		{
			throw new ArgumentNullException(nameof(options));
		}
		DatabaseName = options.Value.DbName;

	}
	public Container CreateFor<T>()
	{
		if (!IdentityMap.TryGetForType<T>(out var containerName))
			throw new InvalidOperationException(
				$"The {nameof(EntityContainerMap)} instance is missing the mapping between the type {typeof(T)} and the Cosmos DB Container name it will be found in." 
				+ Environment.NewLine + 
				$"Use {nameof(ContainerFactory)}.{nameof(IdentityMap)}.{nameof(EntityContainerMap.AddFor)} during Startup, and add all mappings.");
			
		var container = _dbClient.GetContainer(DatabaseName, containerName!);
		return container;
	}
}
