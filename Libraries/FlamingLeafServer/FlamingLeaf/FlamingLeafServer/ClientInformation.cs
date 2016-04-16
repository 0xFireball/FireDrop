using System.Net;

namespace FlamingLeaf.Server
{
    class ClientInformation
    {
        private string _udid;
        private IPEndPoint _clientIp;
        public string UDID
        {
            get
            {
                return _udid;
            }
        }
        public IPEndPoint ClientIP
        {
            get
            {
                return _clientIp;
            }
        }
        public ClientInformation(IPEndPoint clientIp, string id)
        {
            _clientIp = clientIp;
            _udid = id;
        }
        public override int GetHashCode()
        {
            if (ClientIP == null || _udid==null) return 0;
            return (ClientIP+UDID).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ClientInformation other = obj as ClientInformation;
            bool clientsEqual = other.ClientIP.Equals(this.ClientIP);
            return (other != null) && clientsEqual && (other.UDID == this.UDID);
        }
    }
}
