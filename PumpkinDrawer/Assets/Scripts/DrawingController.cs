using JI.Unity.Networking;
using UnityEngine;

namespace JI.Unity.PumpkinCarver.Drawer
{
    public class DrawingController : MonoBehaviour
    {
        public NetworkPeerConnection networkConnection;

        void Start()
        {
            var fullAddress = GameState.HostAddress;

            if (string.IsNullOrEmpty(fullAddress))
            {
                fullAddress = "127.0.0.1:8888";
            }

            var components = fullAddress.Split(':');

            networkConnection.hostSettings.address = components[0];
            networkConnection.hostSettings.port = int.Parse(components[1]);

            networkConnection.ConnectToHost();
        }
    }
}