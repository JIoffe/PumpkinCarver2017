using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace JI.Unity.Networking
{
    public class NetworkPeerConnection : MonoBehaviour
    {
        private static NetworkPeerConnection _Instance;
        public static NetworkPeerConnection Instance { get { return _Instance; } }

        public delegate void NetworkDataEventHandler(string message);

        //Assume we'll only have one network client anyway, so this might as well be
        //static for a slight performance boost

        public static event NetworkDataEventHandler OnNetworkDataEvent;

        [Serializable]
        public class PeerSettings
        {
            [Tooltip("The port to open socket for this peer")]
            public int port = 8000;

            [Tooltip("Max peer-to-peer connections")]
            public int maxConnections = 10;

            [Tooltip("The maximum size of the data buffer - if incoming data exceeds this, an error will be thrown")]
            public int dataBufferSize = 1024;

            [Tooltip("Preferred size of messages to actually send")]
            public int preferredPacketSize = 256;

            [Tooltip("Whether this should act as a server")]
            public bool isDedicatedHost = false;
        }

        [Serializable]
        public class HostSettings
        {
            [Tooltip("The IPv4 address of the target host")]
            public string address;

            [Tooltip("The port on the host to connect to")]
            public int port = 8888;

            [Tooltip("Whether or not this peer should connect to the host right away")]
            public bool shouldConnectOnStart = true;
        }

        public PeerSettings peerSettings;
        public HostSettings hostSettings;

        int channelId;
        int socketId;
        int connectionId = -1;

        ulong outgoingMessageId = 0;

        private byte[] dataBuffer;

        private readonly IDictionary<string, List<string>> _IncomingMessageDictionary = new Dictionary<string, List<string>>();
        private IDictionary<string, List<string>> IncomingMessageDictionary { get { return _IncomingMessageDictionary; } }

        void Awake()
        {
            _Instance = this;
        }

        void Start()
        {
            NetworkTransport.Init();

            var config = new ConnectionConfig();
            channelId = config.AddChannel(QosType.Reliable);

            var topology = new HostTopology(config, peerSettings.maxConnections);

            socketId = NetworkTransport.AddHost(topology, peerSettings.port);
            Debug.Log("Opened NetworkTransport Socket ID " + socketId + " on port " + peerSettings.port);

            dataBuffer = new byte[peerSettings.dataBufferSize];

            if (hostSettings.shouldConnectOnStart && !peerSettings.isDedicatedHost)
            {
                ConnectToHost();
            }
        }

        // Update is called once per frame
        void Update()
        {
            int peerHostId;
            int peerConnectionId;
            int peerChannelId;
            int incomingDataSize;
            byte error;

            var networkEvent = NetworkTransport.Receive(out peerHostId, out peerConnectionId, out peerChannelId, dataBuffer, peerSettings.dataBufferSize, out incomingDataSize, out error);

            switch (networkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("Established connection with peer ID " + peerHostId);
                    break;
                case NetworkEventType.DataEvent:
                    if (OnNetworkDataEvent != null)
                    {
                        string message;
                        message = Encoding.UTF8.GetString(dataBuffer.Take(incomingDataSize).ToArray()).Trim();

                        var pieces = message.Split('$');

                        var transmissionId = pieces[0];
                        if (!IncomingMessageDictionary.ContainsKey(transmissionId))
                        {
                            IncomingMessageDictionary[transmissionId] = new List<string>();
                        }

                        var transmissionHistory = IncomingMessageDictionary[transmissionId];
                        if (pieces.Length == 2)
                        {
                            //Still ongoing message
                            transmissionHistory.Add(pieces[1]);
                        }
                        else
                        {
                            //We are finished!
                            var sb = new StringBuilder();
                            foreach (var msg in transmissionHistory)
                            {
                                sb.Append(msg);
                            }
                            sb.Append(pieces[2]);

                            var completeMessage = sb.ToString();
                            OnNetworkDataEvent.Invoke(completeMessage);

                            IncomingMessageDictionary.Remove(transmissionId);
                        }
                    }
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("Lost connection with peer ID " + peerHostId);
                    break;
                default:
                    break;
            }
        }

        public void ConnectToHost()
        {
            byte error;
            connectionId = NetworkTransport.Connect(socketId, hostSettings.address, hostSettings.port, 0, out error);

            NetworkError trueError = (NetworkError)error;
            if (trueError != NetworkError.Ok)
            {
                Debug.LogError("Could not connect to host: " + Enum.GetName(typeof(NetworkError), trueError));
            }
            else
            {
                Debug.Log("Connected to server. ConnectionId: " + connectionId);
            }
        }

        public void SendSocketMessage(object o)
        {
            SendSocketMessage(JsonUtility.ToJson(o));
        }

        public void SendSocketMessage(string message)
        {
            StartCoroutine(SendSocketMessage_Coroutine(message));
        }

        private IEnumerator SendSocketMessage_Coroutine(string message)
        {
            var messagePrefix = connectionId + "-" + outgoingMessageId++ + "$";
            var prefixPadding = messagePrefix.Length;

            //Split incoming string into chunks
            var chunkSize = peerSettings.preferredPacketSize - prefixPadding;
            var chunks = Chunk(message.ToCharArray(), chunkSize);
            var nChunks = chunks.Count();

            var i = 0;
            do
            {
                string packetString;
                if (i < nChunks - 1)
                {
                    packetString = messagePrefix + new string(chunks.ElementAt(i).ToArray());
                }
                else
                {
                    packetString = messagePrefix + "END$" + new string(chunks.ElementAt(i).ToArray());
                }

                var data = Encoding.UTF8.GetBytes(packetString);

                byte error;
                NetworkTransport.Send(socketId, connectionId, channelId, data, data.Length, out error);

                var trueError = (NetworkError)error;
                if (trueError != NetworkError.Ok)
                {
                    Debug.LogError("Could not send message through channel: " + Enum.GetName(typeof(NetworkError), trueError));
                    break;
                }

                yield return null;
            } while (++i < nChunks);


        }

        private static IEnumerable<IEnumerable<T>> Chunk<T>(T[] src, int chunkSize)
        {
            var n = (int)Math.Ceiling((float)src.Length / chunkSize);
            for (var i = 0; i < n; ++i)
            {
                yield return src.Skip(i * chunkSize).Take(chunkSize);
            }
        }
    }
}