using Newtonsoft.Json;

namespace FlamingLeafToolkit
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NetMessage
    {
        public NetMessage(string data, string username, string secret, string additionalInfo)
        {
            Data = data;
            Username = username;
            Secret = secret;
            AdditionalInfo = additionalInfo;
        }

        [JsonProperty]
        public string Username { get; }

        [JsonProperty]
        public string Secret { get; }

        [JsonProperty]
        public string Data { get; }

        [JsonProperty]
        public string AdditionalInfo { get; }
    }
}