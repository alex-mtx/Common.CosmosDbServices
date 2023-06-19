namespace Mtx.CosmosDbServices.Extensions;

internal static class TransactionalBatchResponseExtensions
{
	public static Result ToResult(this TransactionalBatchResponse me)
	{
		return new Result((int)me.StatusCode);

	}
}
