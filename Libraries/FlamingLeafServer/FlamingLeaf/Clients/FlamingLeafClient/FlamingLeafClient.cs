using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FlamingLeafToolkit;
using Newtonsoft.Json;
using OmniBean.PowerCrypt4.Utilities;

namespace FlamingLeafClient
{
    public class FlamingLeafServiceConnector
    {
        #region Private Fields

        private readonly Thread _heartbeatThread;
        private readonly Thread _receiverThread;
        private CryptUDPClient _client = new CryptUDPClient();
        private IPEndPoint _ep;
        private string _secret;
        private string _serverAddress;
        private int _serverPort;
        // endpoint where server is listening

        private string _username;

        #endregion Private Fields

        #region Public Constructors

        public FlamingLeafServiceConnector(string serverIPaddress, int serverPort, string username="FireDropClient")
        {
            _serverAddress = serverIPaddress;
            _heartbeatThread = new Thread(SendHeartbeats);
            _receiverThread = new Thread(RunListeningThread);
            _serverPort = serverPort;
            _username = username;
            _secret = Guid.NewGuid().ToString("N");
            _ep = new IPEndPoint(IPAddress.Parse(_serverAddress), _serverPort);
        }

        #endregion Public Constructors

        #region Public Properties

        public CryptUDPClient Client => _client;

        #endregion Public Properties

        #region Public Methods

        public void Disconnect()
        {
            var sendDataStr = "disconnect";
            var nMsg = new NetMessage(sendDataStr, _username, _secret, _client.PublicKey);
            var sendStr = JsonConvert.SerializeObject(nMsg);
            var unEncBy = sendStr.GetBytes();
            _client.SendUnencryptedBytes(unEncBy); //send
        }

        public void EstablishConnection()
        {
            _client.Connect(_ep);
            var sendDataStr = "connect"; //this time, sending a connect command
            var sKeyinb64 = Convert.ToBase64String(_client.PublicKey.GetBytes());
            var nMsg = new NetMessage(sendDataStr, _username, _secret, sKeyinb64);
            var sendStr = JsonConvert.SerializeObject(nMsg);
            var ssBytes = sendStr.GetBytes();
            _client.SendUnencryptedBytes(ssBytes); //Send a connect command along with a public key
            var encryptedSessionKey = _client.ReceiveUnencryptedBytes(ref _ep);
            var sessionKey = _client.DecryptBytesWithPrivateKey(encryptedSessionKey).GetString();
            _client.SetSessionKey(_secret, sessionKey);
            OnConnectionEstablished();
            //start services
            _heartbeatThread.Start();
            _receiverThread.Start();
        }

        public void SendData(string data)
        {
            data = data.Remove(0, 3);
            data = _client.EncryptWSessionKey(data.GetBytes(), _secret);
            var nMsg = new NetMessage(data, _username, _secret, _client.PublicKey);
            var sendStr = JsonConvert.SerializeObject(nMsg);
            var sendUnencryptedBytes = sendStr.GetBytes();
            _client.SendUnencryptedBytes(sendUnencryptedBytes); //send
        }

        #endregion Public Methods

        #region Private Methods

        protected virtual void OnConnectionEstablished()
        {
            ConnectionEstablished?.Invoke(this, EventArgs.Empty);
        }

        private void OnDataReceived(FlamingLeafDataReceivedEventArgs args)
        {
            DataReceived?.Invoke(this, args);
        }

        private void RunListeningThread()
        {
            //connect to server
            while (true)
            {
                // then receive data
                //THIS IS THE LISTENER
                var data = _client.ReceiveUnencryptedBytes(ref _ep); //recieve stuff from ep, set to data
                var decData = _client.DecryptWSessionKey(data, _secret);
                OnDataReceived(new FlamingLeafDataReceivedEventArgs(decData));
            }
        }

        private void SendHeartbeats()
        {
            while (true)
            {
                var sendDataStr = "marco";
                sendDataStr = _client.EncryptWSessionKey(sendDataStr.GetBytes(), _secret);
                var nMsg = new NetMessage(sendDataStr, _username, _secret, _client.PublicKey);
                var sendStr = JsonConvert.SerializeObject(nMsg);
                var sendUnencryptedBytes = sendStr.GetBytes();
                _client.SendUnencryptedBytes(sendUnencryptedBytes); //send keepalive
                Thread.Sleep(20000); //Every 20 seconds
            }
        }

        #endregion Private Methods

        #region Public Events

        public event EventHandler ConnectionEstablished;

        public event EventHandler DataReceived;

        #endregion Public Events

        public async void EstablishConnectionAsync()
        {
            await Task.Run(() => EstablishConnection());
        }
    }
}