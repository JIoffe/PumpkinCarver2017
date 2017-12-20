using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JI.Unity.Networking
{
    public class ActiveConnection
    {
        public int peerId { get; set; }
        public int ConnectionId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}