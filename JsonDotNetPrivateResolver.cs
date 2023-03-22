namespace Common.CosmosDbServices
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System.Reflection;
#pragma warning restore CS8603
#pragma warning restore CS8600

    public class JsonDotNetPrivateResolver : DefaultContractResolver
    {
        public JsonDotNetPrivateResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy();
        }

        protected override JsonProperty CreateProperty(MemberInfo member
                                                     , MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }
    }
}
