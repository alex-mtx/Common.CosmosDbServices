namespace Mtx.CosmosDbServices
{
    using Microsoft.Azure.Cosmos;
    using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Newtonsoft.Json.Serialization;
	using System;
	using System.IO;
	using System.Reflection;
	using System.Text;


    /// <summary>
    /// Azure Cosmos DB does not expose a default implementation of CosmosSerializer that is required to set the custom JSON serializer settings.
    /// To fix this, we have to create our own implementation inspired internal implementation from SDK library.
    /// <remarks>
    /// See: https://github.com/Azure/azure-cosmos-dotnet-v3/blob/master/Microsoft.Azure.Cosmos/src/Serializer/CosmosJsonDotNetSerializer.cs
    /// </remarks>
    /// </summary>
#pragma warning disable CS8603
#pragma warning disable CS8600
    public sealed class CosmosJsonDotNetSerializer : CosmosSerializer
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);
        private readonly JsonSerializerSettings _serializerSettings;

        /// <summary>
        /// Create a serializer that uses the JSON.net serializer
        /// </summary>  
        public CosmosJsonDotNetSerializer(JsonSerializerSettings jsonSerializerSettings)
        {
            _serializerSettings = jsonSerializerSettings ??
                  throw new ArgumentNullException(nameof(jsonSerializerSettings));
        }

        /// <summary>
        /// Convert a Stream to the passed in type.
        /// </summary>
        /// <typeparam name="T">The type of object that should be deserialized</typeparam>
        /// <param name="stream">An open stream that is readable that contains JSON</param>
        /// <returns>The object representing the deserialized stream</returns>
        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {

				if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                using (var sr = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        var jsonSerializer = GetSerializer();
						return jsonSerializer.Deserialize<T>(jsonTextReader);
                    }
                }
            }
        }

        /// <summary>
        /// Converts an object to a open readable stream
        /// </summary>
        /// <typeparam name="T">The type of object being serialized</typeparam>
        /// <param name="input">The object to be serialized</param>
        /// <returns>An open readable stream containing the JSON of the serialized object</returns>
        public override Stream ToStream<T>(T input)
        {
            var streamPayload = new MemoryStream();
            using (var streamWriter = new StreamWriter(streamPayload, encoding: DefaultEncoding, bufferSize: 1024, leaveOpen: true))
            {
                using (JsonWriter writer = new JsonTextWriter(streamWriter))
                {
                    writer.Formatting = Formatting.None;
                    var jsonSerializer = GetSerializer();
                    jsonSerializer.Serialize(writer, input);
                    writer.Flush();
                    streamWriter.Flush();
                }
            }

            streamPayload.Position = 0;
            return streamPayload;
        }

        /// <summary>
        /// JsonSerializer has hit a race conditions with custom settings that cause null reference exception.
        /// To avoid the race condition a new JsonSerializer is created for each call
        /// </summary>
        private JsonSerializer GetSerializer()
        {
            return JsonSerializer.Create(_serializerSettings);
        }
    }
	public class PrivateSetterContractResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var jProperty = base.CreateProperty(member, memberSerialization);
			if (jProperty.Writable)
				return jProperty;

			jProperty.Writable = member.IsPropertyWithSetter();

			return jProperty;
		}
	}

	public class PrivateSetterCamelCasePropertyNamesContractResolver : CamelCasePropertyNamesContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var jProperty = base.CreateProperty(member, memberSerialization);
			if (jProperty.Writable)
				return jProperty;

			jProperty.Writable = member.IsPropertyWithSetter();

			return jProperty;
		}
	}

	internal static class MemberInfoExtensions
	{
		internal static bool IsPropertyWithSetter(this MemberInfo member)
		{
			var property = member as PropertyInfo;

			return property?.GetSetMethod(true) != null;
		}
	}

	public class Serialization
	{
		public static string Serialize(object? instance)
		{
			var serialization = JsonConvert.SerializeObject(instance, new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			});
			return serialization;
		}

		public static T Deserialize<T>(string serialization)
		{
			var deserialized = JsonConvert.DeserializeObject<T>(serialization, new JsonSerializerSettings
			{
				//ContractResolver = new CamelCasePropertyNamesContractResolver()
				ContractResolver = new PrivateSetterCamelCasePropertyNamesContractResolver()
			});
			return deserialized!;
		}

		public static object Deserialize(string serialization, Type type)
		{
			var deserialized = JsonConvert.DeserializeObject(serialization, type, new JsonSerializerSettings
			{
				//ContractResolver = new CamelCasePropertyNamesContractResolver()
				ContractResolver = new PrivateSetterCamelCasePropertyNamesContractResolver()
			});

			return deserialized!;
		}
	}
}
