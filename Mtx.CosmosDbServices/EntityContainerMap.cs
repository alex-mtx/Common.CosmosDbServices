namespace Mtx.CosmosDbServices;

public class EntityContainerMap
{

	private Dictionary<Type, ContainerName> mappings = new ();

	public void AddFor<T>(ContainerName containerName)
	{
		mappings[typeof(T)] = containerName;
	}

	public ContainerName GetForType<T>()
	{
		return mappings[typeof(T)];
	}

	public bool TryGetForType<T>(out ContainerName? containerName)
	{
		return mappings.TryGetValue(typeof(T),out containerName);
	}

}
