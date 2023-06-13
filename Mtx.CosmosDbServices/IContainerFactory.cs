namespace Mtx.CosmosDbServices;

internal interface IContainerFactory
{
	Container CreateFor<T>();
}
