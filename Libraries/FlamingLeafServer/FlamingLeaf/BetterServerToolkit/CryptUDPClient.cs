using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using OmniBean.PowerCrypt4;
using OmniBean.PowerCrypt4.Utilities;

namespace FlamingLeafToolkit
{
    public class CryptUDPClient
    {
        private IPEndPoint _remoteIp;
        private PowerRSA _rsaProvider;
        private readonly UdpClient _udpClient;
        private Dictionary<string, string> _sessionKeyDict;

        public CryptUDPClient()
        {
            _udpClient = new UdpClient();
            _rsaProvider = new PowerRSA(1024);
            DoNormalInit();
        }

        public CryptUDPClient(int port)
        {
            _udpClient = new UdpClient(port);
            _rsaProvider = new PowerRSA(1024);
            DoNormalInit();
        }

        public CryptUDPClient(string publicKey)
        {
            _udpClient = new UdpClient();
            _rsaProvider = new PowerRSA(publicKey, 1024);
            DoNormalInit();
        }

        public CryptUDPClient(string publicKey, int port)
        {
            _udpClient = new UdpClient(port);
            _rsaProvider = new PowerRSA(publicKey, 1024);
            DoNormalInit();
        }

        public string PublicKey
        {
            get { return _rsaProvider.PublicKey; }
        }

        public UdpClient Client
        {
            get { return _udpClient; }
        }

        private void DoNormalInit()
        {
            _sessionKeyDict = new Dictionary<string, string>();
        }

        public void ReinitializeRSA(string publicKey)
        {
            _rsaProvider = new PowerRSA(publicKey, 1024);
        }

        public void Connect(IPEndPoint kRemoteIP)
        {
            _remoteIp = kRemoteIP;
            _udpClient.Connect(_remoteIp);
        }

        public string GetSessionKey(string secret)
        {
            return _sessionKeyDict[secret];
        }

        public bool SetSessionKey(string secret, string kSessionKey)
        {
            if (!_sessionKeyDict.ContainsKey(secret))
                _sessionKeyDict.Add(secret, kSessionKey);
            else
                _sessionKeyDict[secret] = kSessionKey;
            return true;
        }

        public void UnregisterSessionKey(string secret)
        {
            _sessionKeyDict.Remove(secret);
        }

        public void SendUnencryptedBytes(byte[] data)
        {
            _udpClient.Send(data, data.Length);
        }

        public void SendUnencryptedBytes(byte[] data, IPEndPoint remoteEP)
        {
            _udpClient.Send(data, data.Length, remoteEP);
        }

        public byte[] ReceiveUnencryptedBytes(ref IPEndPoint remoteEP)
        {
            var rawBytes = _udpClient.Receive(ref remoteEP);
            return rawBytes;
        }

        public string EncryptWSessionKey(byte[] raw, string secret)
        {
            return PowerAES.Encrypt(raw.GetString(), _sessionKeyDict[secret]);
        }

        public string DecryptWSessionKey(byte[] encrypted, string secret)
        {
            return PowerAES.Decrypt(encrypted.GetString(), _sessionKeyDict[secret]);
        }

        public byte[] EncryptBytesWithPublicKey(byte[] data)
        {
            var pubEncryptedBytes = _rsaProvider.EncryptStringWithPublicKey(data.GetString()).GetBytes();
            //_udpClient.Send(pubEncryptedBytes, pubEncryptedBytes.Length);
            return pubEncryptedBytes;
        }

        public byte[] DecryptBytesWithPrivateKey(byte[] encryptedBytes)
        {
            //byte[] rawData = _udpClient.Receive(ref remoteEP);
            return _rsaProvider.DecryptStringWithPrivateKey(encryptedBytes.GetString()).GetBytes();
        }

        public void Close()
        {
            _udpClient.Close();
        }
    }
}