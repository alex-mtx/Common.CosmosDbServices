
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Common.CosmosDbServices
{
    public static class CosmosDbConfigOptionsExtensions
    {
        /// <summary>
        /// Creates a Cosmos DB database and a container with the specified partition key. 
        /// </summary>
        /// <returns></returns>
        public static void InitializeCosmosCosmosDbService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<CosmosClient>(factory =>
            {
                var options = factory.GetRequiredService<IOptions<CosmosDbOptions>>().Value;

                var clientOptions = new CosmosClientOptions
                {
                    //SerializerOptions = options,
                    Serializer = new CosmosJsonDotNetSerializer(new JsonSerializerSettings
                    {

                        ContractResolver = new JsonDotNetPrivateResolver(),
                        TypeNameHandling = TypeNameHandling.None,
                        ReferenceLoopHandling = ReferenceLoopHandling.Error,
                        PreserveReferencesHandling = PreserveReferencesHandling.None,
                        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    })

                };

                return new CosmosClient(options.Endpoint, options.Key,clientOptions: clientOptions);
            });
        }
    }

}
