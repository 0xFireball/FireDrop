using System.Net;

namespace FlamingLeafToolkit
{
    public class FlamingApiClientInformation
    {
        public FlamingApiClientInformation(string username, string secret, string sessionKey, IPEndPoint clientIp)
        {
            Username = username;
            Secret = secret;
            SessionKey = sessionKey;
            IPAddress = clientIp;
        }

        public string Username { get; }

        public string Secret { get; }

        public string SessionKey { get; }

        public IPEndPoint IPAddress { get; }
    }
}