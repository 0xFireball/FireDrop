﻿using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using OmniBean.PowerCrypt4;
using OmniBean.PowerCrypt4.Utilities;

namespace FlamingLeafToolkit
{
    public class CryptUDPClient
    {
        private IPEndPoint _remoteIP;
        private PowerRSA _rsaProvider;
        private readonly UdpClient _udpClient;
        private string sessionKey;
        private Dictionary<string, string> sessionKeyDict;

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

        private void DoNormalInit()
        {
            sessionKeyDict = new Dictionary<string, string>();
        }

        public void ReinitializeRSA(string publicKey)
        {
            _rsaProvider = new PowerRSA(publicKey, 1024);
        }

        public void Connect(IPEndPoint kRemoteIP)
        {
            _remoteIP = kRemoteIP;
            _udpClient.Connect(_remoteIP);
        }

        public string GetSessionKey(string secret)
        {
            return sessionKeyDict[secret];
        }

        public bool SetSessionKey(string secret, string kSessionKey)
        {
            if (!sessionKeyDict.ContainsKey(secret))
                sessionKeyDict.Add(secret, kSessionKey);
            else
                sessionKeyDict[secret] = kSessionKey;
            return true;
            //sessionKey = kSessionKey;
        }

        public void UnregisterSessionKey(string secret)
        {
            sessionKeyDict.Remove(secret);
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

        public void SendBytes(byte[] data)
        {
            var sEncBy = PowerAES.Encrypt(data.GetString(), sessionKey).GetBytes();
            _udpClient.Send(sEncBy, sEncBy.Length);
        }

        public void SendBytes(byte[] data, IPEndPoint remoteEP)
        {
            var sEncBy = PowerAES.Encrypt(data.GetString(), sessionKey).GetBytes();
            _udpClient.Send(sEncBy, sEncBy.Length, remoteEP);
        }

        public byte[] ReceiveBytes(ref IPEndPoint remoteEP)
        {
            var rawBytes = _udpClient.Receive(ref remoteEP);
            var sDecBy = PowerAES.Decrypt(rawBytes.GetString(), sessionKey).GetBytes();
            return sDecBy;
        }

        public string EncryptWSessionKey(byte[] raw, string secret)
        {
            return PowerAES.Encrypt(raw.GetString(), sessionKeyDict[secret]);
        }

        public string DecryptWSessionKey(byte[] encrypted, string secret)
        {
            return PowerAES.Decrypt(encrypted.GetString(), sessionKeyDict[secret]);
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
    }
}