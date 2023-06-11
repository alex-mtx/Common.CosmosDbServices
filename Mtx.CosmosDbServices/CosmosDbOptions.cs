namespace Mtx.CosmosDbServices;


public record CosmosDbOptions
{
    public const string CosmosDb = "CosmosDb";
    public string Key { get; set; }
    public string Endpoint { get; set; }
    public string DbName { get; set; }
}