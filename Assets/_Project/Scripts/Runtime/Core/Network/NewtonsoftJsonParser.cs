using Newtonsoft.Json;

namespace TestTask.Core.Network
{
    public sealed class NewtonsoftJsonParser : IJsonParser
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public T Parse<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }
    }
}
