namespace Mtx.CosmosDbServices.Extensions;

internal static class ItemResponseExtensions
{
	public static Result ToResult<T>(this ItemResponse<T> me)
	{
		return new Result((int)me.StatusCode);

	}
}
