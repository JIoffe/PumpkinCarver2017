using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace JI.Unity.Networking
{
    public class PeerSocketConnection : MonoBehaviour
    {
        /// <summary>
        /// Delegate invoked when data is received from another peer
        /// </summary>
        public delegate void NetworkDataEventHandler(object payload);
        /// <summary>
        /// Delegate invoked when a connection is made with another peer
        /// </summary>
        public delegate void NetworkConnectEventHandler(int peerId, int connectionId);
        /// <summary>
        /// Delegate invoked when a connection is made with another peer
        /// </summary>
        public delegate void NetworkDisconnectEventHandler(int peerId, int connectionId);

        [Serializable]
        public class ConnectionSettings
        {
            /// <summary>
            /// Maximumum number of concurrent connections to allow
            /// </summary>
            [Tooltip("Maximumum number of concurrent connections to allow")]
            public int maxConcurrentConnections = 10;

            /// <summary>
            /// Maximum size accepted for incoming data (in bytes)
            /// </summary>
            [Tooltip("Maximum size accepted for incoming data (in bytes)")]
            public int incomingDataBufferSize = 1024;

            /// <summary>
            /// Port that this peer will listen on for incoming connections
            /// </summary>
            [Tooltip("Port that this peer will listen on for incoming connections")]
            public int listeningPort = 8888;

            /// <summary>
            /// Primary channel type used to communicate between peers
            /// </summary>
            [Tooltip("Primary channel type used to communicate between peers")]
            public QosType channelQosType = QosType.Reliable;
        }

        //Unity GUI-visible properties
        public ConnectionSettings connectionSettings;
        public string[] initialPeerConnections;
        public bool initializeSocketOnStart = true;

        public event NetworkDataEventHandler OnNetworkDataEvent;
        public event NetworkConnectEventHandler OnNetworkConnectEvent;
        public event NetworkDisconnectEventHandler OnNetworkDisconnectEvent;

        /// <summary>
        /// List of active Peer Connections
        /// </summary>
        private IList<ActiveConnection> ActiveConnections
        {
            get
            {
                if (_ActiveConnections == null)
                    return Enumerable.Empty<ActiveConnection>().ToArray();

                return _ActiveConnections;
            }
        }
        private IList<ActiveConnection> _ActiveConnections;

        /// <summary>
        /// Buffer used for incoming data from other peers. Size is fixed to ConnectionSettings.incomingDataBufferSize
        /// </summary>
        private byte[] DataBuffer
        {
            get
            {
                if (_DataBuffer == null)
                    _DataBuffer = new byte[connectionSettings.incomingDataBufferSize];

                return _DataBuffer;
            }
        }
        private byte[] _DataBuffer;

        private bool hasOpenedSocket = false;

        private int channelId;
        /// <summary>
        /// ID of active socket opened on this peer
        /// </summary>
        private int socketId;

        // Use this for initialization
        void Start()
        {
            if (initializeSocketOnStart)
                OpenSocket();
        }

        // Update is called once per frame
        void Update()
        {
            int peerHostId;
            int peerConnectionId;
            int peerChannelId;
            int incomingDataSize;
            byte error;

            var networkEvent = NetworkTransport.Receive(out peerHostId, out peerConnectionId, out peerChannelId, DataBuffer, connectionSettings.incomingDataBufferSize, out incomingDataSize, out error);

            switch (networkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log(string.Format("Established Connection: Peer ID [{0}], Connection ID [{1}]", peerHostId, peerConnectionId));
                    if (OnNetworkConnectEvent != null)
                        OnNetworkConnectEvent.Invoke(peerHostId, peerConnectionId);
                    break;

                case NetworkEventType.DisconnectEvent:
                    Debug.Log(string.Format("Lost Connection: Peer ID [{0}], Connection ID [{1}]", peerHostId, peerConnectionId));
                    if (OnNetworkDisconnectEvent != null)
                        OnNetworkDisconnectEvent.Invoke(peerHostId, peerConnectionId);
                    break;

                case NetworkEventType.DataEvent:
                    break;

                default:
                    break;
            }
        }

        public void OpenSocket()
        {
            if (hasOpenedSocket)
                return;

            NetworkTransport.Init();
            var topology = CreateHostTopology();

            var port = connectionSettings.listeningPort;
            socketId = NetworkTransport.AddHost(topology, port);
            Debug.Log(string.Format("Opened NetworkTransport Socket ID [{0}] on port [{1}]", socketId, port));
        }

        public void Connect(string address)
        {
            var segments = address.Trim().Split(':');
            if(segments.Length != 2)
            {
                Debug.LogError("Invalid address: " + address);
                return;
            }

            var host = segments[0];
            int port;
            if(!int.TryParse(segments[1], out port))
            {
                Debug.LogError(string.Format("Cannot connect to [{0}]: Invalid Port", address));
                return;
            }

            Connect(host, port);
        }

        public void Connect(string host, int port)
        {

        }

        public void SendMessage(object message)
        {

        }

        private HostTopology CreateHostTopology()
        {
            var config = new ConnectionConfig();
            channelId = config.AddChannel(connectionSettings.channelQosType);

            return new HostTopology(config, connectionSettings.maxConcurrentConnections);
        }
        
        //private void RegisterPeer(int peerId, int)

        private IEnumerator SendMessage_Coroutine()
        {
            //Two step coroutine
            // 1) Prepare message
            // 2) Send to all connected peers

            //Step 2 send packets to all peers
            var n = ActiveConnections.Count;
            for(var i = 0; i < n; ++i)
            {
                
                yield return null;
            }
        }
    }
}