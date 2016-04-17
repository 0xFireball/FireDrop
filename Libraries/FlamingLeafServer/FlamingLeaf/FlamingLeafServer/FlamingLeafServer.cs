using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Aluminum.PluginCore2;
using Aluminum.PluginCore2.Types;
using FlamingLeaf.Api;
using FlamingLeafToolkit;
using Newtonsoft.Json;
using OmniBean.PowerCrypt4.Utilities;
using PluginInterface;

namespace FlamingLeaf.Server
{
    public class FlamingLeafServer : IFlamingPluginHost
    {
        #region Private Fields

        private ConnectedClient _lastClient;
        private string _lastResponse;
        private Thread _purgeClientsThread;
        private Thread _routerThread;
        private int _serverPort;
        private byte[] data;
        private FlamingPluginHostController hostController;
        private List<Func<FlamingApiRequest, FlamingApiClientInformation, int>> pluginCallbackSender;
        private IPEndPoint remoteIP;

        //Thread t1 = new Thread(new ThreadStart(UDPListener.listener));
        private Dictionary<ClientInformation, ConnectedClient> sClientDict;

        private CryptUDPClient udpServer;
        private TimeSpan waitTime;

        #endregion Private Fields

        #region Public Constructors

        public FlamingLeafServer(int port)
        {
            _serverPort = port;
            remoteIP = new IPEndPoint(IPAddress.Any, _serverPort);
            udpServer = new CryptUDPClient(_serverPort);

            hostController = new FlamingPluginHostController();

            sClientDict = new Dictionary<ClientInformation, ConnectedClient>();
            waitTime = TimeSpan.FromMinutes(2.0);

            pluginCallbackSender = new List<Func<FlamingApiRequest, FlamingApiClientInformation, int>>();
            PreparePlugins();
        }

        public string HostAddress => GetLocalIPv4(NetworkInterfaceType.Wireless80211);

        public string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            try
            {
                foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                    {
                        foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                output = ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            catch (NetworkInformationException)
            {
                output = "";
            }
            return output;
        }

        #endregion Public Constructors

        #region Public Methods

        public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>
       (Dictionary<TKey, TValue> original) where TValue : ICloneable
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count,
                                                                    original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ret.Add(entry.Key, (TValue)entry.Value.Clone());
            }
            return ret;
        }

        public static bool IsJson(string input)
        {
            input = input.Trim();
            return input.StartsWith("{") && input.EndsWith("}")
                   || input.StartsWith("[") && input.EndsWith("]");
        }

        public static Thread SpawnNewThread(Action threadAction)
        {
            var nT = new Thread(() => threadAction());
            nT.Start();
            return nT;
        }

        public void BroadcastData(string data)
        {
            BroadcastDataToAllClients(data);
        }

        public string CraftApiRequest(FlamingApiRequest apiRequest)
        {
            string rJson = JsonConvert.SerializeObject(apiRequest);
            string j64 = Convert.ToBase64String(rJson.GetBytes());
            return j64;
        }

        public void HandleReceivedData(IPEndPoint clientIP, byte[] data)
        {
            if (data.GetString() == "HANDSHAKE")
            {
                //Perform handshake
            }
            string strRespJson = data.GetString();
            NetMessage netMsg = null;
            string clientUsername = null;
            string cMessage = null;
            string cSecret = null;
            string cParams = null;
            if (IsJson(strRespJson))
            {
                try
                {
                    netMsg = JsonConvert.DeserializeObject<NetMessage>(strRespJson);
                    clientUsername = netMsg.Username;
                    cMessage = netMsg.Data;
                    cSecret = netMsg.Secret;
                    try
                    {
                        cParams = Convert.FromBase64String(netMsg.AdditionalInfo).GetString();
                    }
                    catch
                    {
                        //cparams are not used
                    }
                    /*
	                Console.WriteLine(clientUsername);
	                Console.WriteLine(cMessage);
	                Console.WriteLine(cSecret);
	                Console.WriteLine(cParams);
	                */
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Problematic string: {0}", strRespJson);
                    Console.WriteLine("RandomDeserializeException: " + ex);
                }
            }
            lock (sClientDict)
            {
                var clientInfo = new ClientInformation(clientIP, cSecret);
                //Console.WriteLine("{ "+clientIP+" : "+cParam+" }");
                ConnectedClient connClient;
                string clientId = null;
                switch (cMessage)
                {
                    case "connect":
                        clientId = cSecret;
                        Console.WriteLine("Connect command from: " + clientIP.Address.ToString());
                        /*Console.WriteLine(cParams);*/
                        connClient = new ConnectedClient(udpServer, clientIP, clientId, cParams, clientUsername);
                        connClient.TalkTosClient();
                        connClient.SendRawStringToClient(connClient.encSessionKey.GetString());
                        connClient.SendNonJSONStringToClient("OK");
                        connClient.SendNonJSONStringToClient("Welcome. You have been authenticated. There are {0} other clients currently online. Your secret ID is: {1}", sClientDict.Keys.Count, cSecret);
                        connClient.LogCurrentTime();
                        BroadcastDataToAllClients("{0} Logged in.", clientUsername);
                        if (!sClientDict.ContainsKey(clientInfo))
                            sClientDict.Add(clientInfo, connClient);
                        return; //This actually returns
                    case "disconnect":
                        Console.WriteLine("Unencrypted Disconnect command from: " + clientIP.Address.ToString());
                        var connCliObj = sClientDict[clientInfo];
                        connCliObj.KillsClientConnection(cSecret);
                        lock (sClientDict)
                            sClientDict.Remove(clientInfo);
                        return; //This actually returns
                }
                if (!sClientDict.ContainsKey(clientInfo))
                {
                    Console.WriteLine("[Master Thread] receive unauthenticated data from " + clientIP.ToString());
                    Console.WriteLine(data.GetString());
                }
                else
                {
                    if (!sClientDict.ContainsKey(clientInfo)) //This means that the person was purged
                        return;
                    var connCliObj = sClientDict[clientInfo];

                    //Start callback thread for received data
                    SpawnNewThread(() => connCliObj.OnDataReceived(data));

                    string decryptedMessage = connCliObj.udpServer.DecryptWSessionKey(cMessage.GetBytes(), connCliObj.Secret);
                    switch (decryptedMessage)
                    {
                        case "ping":
                            Console.WriteLine("Pinged by {0}", clientUsername);
                            connCliObj.SendNonJSONStringToClient("pong");
                            break;

                        case "marco":
                            connCliObj.LogCurrentTime();
                            break;

                        case "disconnect":
                            Console.WriteLine("Disconnect command from: " + clientIP.Address.ToString());
                            connCliObj.SendNonJSONStringToClient("You have been disconnected.");
                            BroadcastDataToAllClients("{0} Disconnected from server.", clientUsername);
                            connCliObj.KillsClientConnection(cSecret);
                            lock (sClientDict)
                                sClientDict.Remove(clientInfo);
                            return; //This actually returns
                        default:
                            //Pass on to plugins
                            //If the decrypted message is JSON in the form of a FlamingApiRequest, send callbacks
                            string jsonApiRequest = null;
                            try
                            {
                                jsonApiRequest = Convert.FromBase64String(decryptedMessage).GetString();
                            }
                            catch (FormatException)
                            {
                                //Format Error
                            }
                            if (jsonApiRequest != null)
                            {
                                if (IsJson(jsonApiRequest))
                                {
                                    string sessionKey = connCliObj.udpServer.GetSessionKey(connCliObj.Secret);
                                    var clientApiRInfo = new FlamingApiClientInformation(clientUsername, connCliObj.Secret, sessionKey, clientIP);
                                    SetLastClient(connCliObj); //Set client for special callbacks
                                    var apiRequest = JsonConvert.DeserializeObject<FlamingApiRequest>(jsonApiRequest);
                                    foreach (Func<FlamingApiRequest, FlamingApiClientInformation, int> callback in pluginCallbackSender)
                                    {
                                        SendPluginCallback(clientApiRInfo, apiRequest, callback);
                                    }
                                }
                            }
                            else
                            {
                                string bcMessage = string.Format("[BCAST] <{0}>: {1}", clientUsername, decryptedMessage);
                                BroadcastDataToAllClients(bcMessage);
                            }
                            break;
                    }
                    PurgeInactiveClients();
                }
            }
        }

        public void KeepPurgingClients()
        {
            Console.WriteLine("Inactive client purging Thread started.");
            while (true)
            {
                int msWaitTime = (int)(waitTime.TotalSeconds * 1000 / 2);
                Thread.Sleep(msWaitTime);
                PurgeInactiveClients();
            }
        }

        public string ObjectToJson(object someObject)
        {
            return JsonConvert.SerializeObject(someObject);
        }

        public void RunServer()
        {
            Console.ForegroundColor = ConsoleColor.White;
            SpawnNewThread(() =>
            {
                Console.WriteLine("FlamingLeaf Server - v4.4.0");
                Console.WriteLine("Server Running...");
            });

            //Start server thread daemons
            _purgeClientsThread = SpawnNewThread(KeepPurgingClients);
            _routerThread = SpawnNewThread(routerd);
        }

        public async void RunServerAsync()
        {
            await Task.Run(() => RunServer());
        }

        public void SendLastMessageToLastClient()
        {
            _lastClient.SendNonJSONStringToClient(_lastResponse);
        }

        public void SetLastClient(ConnectedClient client)
        {
            _lastClient = client;
        }

        public void SetLastResponse(string j64_response)
        {
            _lastResponse = j64_response;
        }

        public void StopServer()
        {
            _routerThread.Abort();
            _purgeClientsThread.Abort();
            udpServer.Close();
            udpServer = null;
        }

        #endregion Public Methods

        #region Private Methods

        private void __internal_bcastDataToAllClients(string format, params object[] args)
        {
            var mCDictModify = new Dictionary<ClientInformation, ConnectedClient>(sClientDict);
            string strData = string.Format(format, args);
            foreach (var clientEntry in mCDictModify)
            {
                var _theClient = clientEntry.Value;
                Console.WriteLine("Sending data to: key-> {0} [{1}]", _theClient.Secret, strData);
                _theClient.SendNonJSONStringToClient(strData);
            }
        }

        private void BroadcastDataToAllClients(string format, params object[] args)
        {
            SpawnNewThread(() => __internal_bcastDataToAllClients(format, args));
        }

        private void Panic(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
            Console.WriteLine("Running panic sequence...");
            Console.WriteLine("Shutting down plugins...");
            Global.Plugins.ClosePlugins();
        }

        private void PreparePlugins()
        {
            //Call the find plugins routine, to search in our Plugins Folder
            Console.WriteLine("Initializing Plugins...");
            string startupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Global.Plugins.FindPluginsByPath(startupPath + @"\Plugins");
            Global.Plugins.FindPluginsBySuffix(startupPath, ".Plugin.dll");
            var availablePlugins = Global.Plugins.AvailablePlugins;
            //Add each plugin to the treeview
            foreach (AvailablePlugin plugin in availablePlugins)
            {
                Console.WriteLine("[PLUGIN] Loading {0}...", plugin.Instance.Name);
                plugin.Instance.LoadPlugin();
                plugin.Instance.Host = this;
                plugin.Instance.HostObject = hostController;
                pluginCallbackSender.Add(new Func<FlamingApiRequest, FlamingApiClientInformation, int>(plugin.Instance.InvokePlugin));
            }
        }

        private void PurgeInactiveClients()
        {
            var mCDictModify = new Dictionary<ClientInformation, ConnectedClient>(sClientDict);
            foreach (var clientEntry in sClientDict)
            {
                var _theClient = clientEntry.Value;
                DateTime lastReply = _theClient.GetLastActiveTime();
                DateTime rn = DateTime.Now;
                TimeSpan intervalFromLastReply = (rn - lastReply);

                if (intervalFromLastReply > waitTime)
                {
                    Console.WriteLine("{0} was disconnected due to inactivity.", remoteIP.Address.ToString());
                    _theClient.SendNonJSONStringToClient("You have been disconnected.");
                    BroadcastDataToAllClients("{0} was disconnected due to inactivity.", _theClient.DisplayName);
                    _theClient.KillsClientConnection(_theClient.Secret);
                    ClientInformation ci = new ClientInformation(_theClient.RemoteEndpoint, _theClient.Secret);
                    mCDictModify.Remove(ci);
                }
            }
            lock (sClientDict)
                sClientDict = mCDictModify;
        }

        private void routerd()
        {
            //Router Thread
            Console.WriteLine("Router Thread started.");
            while (true)
            {
                Console.WriteLine("[Router] Listening for connections...");
                data = udpServer.ReceiveUnencryptedBytes(ref remoteIP); //listen for incoming data from remoteIP, set data to it
                                                                        //This spawns a new thread to handle received data
                var clientIP = remoteIP.Copy();
                var clientData = data.Copy();
                var ccol = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("[RAWRECV]:{0} - {1}", clientIP, data.GetString());
                Console.ForegroundColor = ccol;
                new Thread(() => HandleReceivedData(clientIP, clientData)).Start();
            }
        }

        private void SendPluginCallback(FlamingApiClientInformation flamingApiClientInformation, FlamingApiRequest apiRequest, Func<FlamingApiRequest, FlamingApiClientInformation, int> callbackMethod)
        {
            int returnValue = callbackMethod(apiRequest, flamingApiClientInformation);
            //Notify on non-zero return value
        }

        #endregion Private Methods
    }

    internal class FlamingPluginHostController
    {
    }
}