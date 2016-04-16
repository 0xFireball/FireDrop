using System;
using System.Net;
using System.Threading;
using FlamingLeafToolkit;
using Newtonsoft.Json;
using OmniBean.PowerCrypt4.Utilities;

namespace FlamingLeaf.Server
{
    public class ConnectedClient
    {
        private IPEndPoint remoteIP;
        public CryptUDPClient udpServer;
        private string _secret;
        private int msDelay = 10;
        private bool killSwitch;
        private string _username;
        private DateTime lastReceivedTime;

        public byte[] encSessionKey;

        public string Secret
        {
            get
            {
                return _secret;
            }
        }

        public IPEndPoint RemoteEndpoint
        {
            get
            {
                return remoteIP;
            }
        }

        public string DisplayName
        {
            get
            {
                return _username;
            }
        }

        public ConnectedClient(CryptUDPClient connectedClient, IPEndPoint serverEnd, string mySecret, string clientKey, string username)
        {
            _secret = mySecret;
            killSwitch = false;
            udpServer = connectedClient;
            //udpServer.Connect(serverEnd);
            //Send the client a session key
            _username = username;
            Console.WriteLine("Encrypting session key with PublicKey {0}", clientKey);
            string sessionKey = OmniBean.PowerCrypt4.PowerAES.GenerateRandomString(24);
            bool setSessionKey = udpServer.SetSessionKey(mySecret, sessionKey);
            udpServer.ReinitializeRSA(clientKey);
            encSessionKey = udpServer.EncryptBytesWithPublicKey(sessionKey.GetBytes());

            //this.SendStringToClient(encSessionKey.GetString());
            //udpServer.SendUnencryptedBytes(udpServer.EncryptBytesWithPublicKey(sessionKey.GetBytes()));

            //udpServer = new CryptUDPClient(serverEnd.Port);
            remoteIP = serverEnd;
        }

        public void TalkTosClient()
        {
            /*
            Thread talkThread = new Thread(new ThreadStart(KeepTalkingTosClient));
            talkThread.Start();
            */
        }

        public void KillsClientConnection(string username)
        {
            udpServer.UnregisterSessionKey(Secret);
            killSwitch = true;
        }

        public void LogCurrentTime()
        {
            //Record last received time
            lastReceivedTime = DateTime.Now;
        }

        public DateTime GetLastActiveTime()
        {
            return lastReceivedTime;
        }

        public void OnDataReceived(byte[] data)
        {
            string strRespJson = data.GetString();
            NetMessage netMsg = null;
            string clientUsername = null;
            string cMessage = null;
            try
            {
                netMsg = JsonConvert.DeserializeObject<NetMessage>(strRespJson);
                clientUsername = netMsg.Username;
                cMessage = udpServer.DecryptWSessionKey(netMsg.Data.GetBytes(), _secret);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RandomDeserializeException: " + ex);
            }

            //Record last received time
            lastReceivedTime = DateTime.Now;
            //Talk to the sClient
            string username = netMsg.Username;
            string secret = netMsg.Secret;
            string cCommand = netMsg.Data;
            cCommand = udpServer.DecryptWSessionKey(cCommand.GetBytes(), _secret);
            if (secret == _secret)
            {
                var pcol = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Client Callback Thread] receive data from {" + username + "}" + remoteIP.ToString());
                Console.WriteLine(cCommand);
                Console.ForegroundColor = pcol;
            }
            Thread.Sleep(msDelay);
        }

        public void SendRawStringToClient(string format, params object[] args)
        {
            string strData = string.Format(format, args);
            byte[] data = strData.GetBytes();
            udpServer.SendUnencryptedBytes(data, remoteIP);
        }

        public void SendStringToClient(string format, params object[] args)
        {
            string strData = string.Format(format, args);
            byte[] data = strData.GetBytes();
            udpServer.SendUnencryptedBytes(data, remoteIP);
        }

        public void SendNonJSONStringToClient(string format, params object[] args)
        {
            FlamingLeafServer.SpawnNewThread(() => __internal_sendnJsonStoClient(format, args));
        }

        private void __internal_sendnJsonStoClient(string format, params object[] args)
        {
            string strData = string.Format(format, args);
            byte[] data = strData.GetBytes();
            udpServer.SendUnencryptedBytes(udpServer.EncryptWSessionKey(data, _secret).GetBytes(), remoteIP);
        }

        private void KeepTalkingTosClient()
        {
            //REMOVE THIS IF THIS IS ACTUALLY USED
            Thread.Sleep(-1);
            while (!killSwitch)
            {
                //Talk to the sClient
            }
        }
    }
}