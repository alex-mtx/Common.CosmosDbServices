namespace Mtx.CosmosDbServices;

public interface IContainerFactory
{
	Container CreateFor<T>();
}
