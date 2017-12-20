using JI.Unity.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JI.Unity.PumpkinCarver.Drawer
{
    public class ShapeCollectionSync : MonoBehaviour
    {
        [Serializable]
        private struct Vec2Serialization
        {
            public float x;
            public float y;
        }

        [Serializable]
        private struct ShapeCollectionSerialization
        {
            public ShapeSerialization[] s;
        }

        [Serializable]
        private struct ShapeSerialization
        {
            public Vec2Serialization[] d;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ClearCollection()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void DispatchShapeCollection()
        {
            var drawnShapes = GetComponentsInChildren<MeshFilter>();
            var shapeSerializations = drawnShapes.Select(s =>
            {
                var d = s.mesh.vertices.Select(v => new Vec2Serialization
                {
                    x = v.x + s.transform.position.x,
                    y = v.y + s.transform.position.y
                }).ToArray();

                return new ShapeSerialization
                {
                    d = d
                };
            }).ToArray();

            var shapeCollection = new ShapeCollectionSerialization
            {
                s = shapeSerializations
            };

            var json = JsonUtility.ToJson(shapeCollection);
            NetworkPeerConnection.Instance.SendSocketMessage(json);
        }
    }
}