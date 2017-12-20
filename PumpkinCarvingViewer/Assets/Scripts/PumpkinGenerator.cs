using JI.Unity.Networking;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace JI.Unity.PumpkinCarver.Viewer
{
    public class PumpkinGenerator : MonoBehaviour
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

        [Tooltip("The prefab to use as a template for generating the face")]
        public GameObject pumpkinPrefab;

        [Tooltip("Scale factor for incoming shapes")]
        public float shapeScale = 1f;

        [Tooltip("How far offset will pumpkins spawn when compared ")]
        public Vector3 pumpkinSpawnOffset = Vector3.zero;

        // Use this for initialization
        void Start()
        {
            NetworkPeerConnection.OnNetworkDataEvent += OnNetworkDataEvent;
        }

        private void OnDestroy()
        {
            NetworkPeerConnection.OnNetworkDataEvent -= OnNetworkDataEvent;
        }

        public void OnNetworkDataEvent(string message)
        {
            var shapeCollection = JsonUtility.FromJson<ShapeCollectionSerialization>(message);
            StartCoroutine(ShapeGeneration_Coroutine(shapeCollection));
        }

        IEnumerator ShapeGeneration_Coroutine(ShapeCollectionSerialization shapeCollection)
        {
            var pumpkin = Instantiate(pumpkinPrefab);
            pumpkin.SetActive(false);

            pumpkin.transform.position = transform.position + pumpkinSpawnOffset;
            var drawnShapeAnchor = pumpkin.GetComponentInChildren<DrawnShape>();
            var anchorPosition = drawnShapeAnchor.transform.localPosition;
            foreach (var shape in shapeCollection.s)
            {
                var points = shape.d.Select(v =>
                {
                    var z = Mathf.Pow(v.x, 2) * 0.002f;
                    return new Vector3(v.x * shapeScale, v.y * shapeScale, z);

                })
                .ToList();

                var drawnShape = Instantiate(drawnShapeAnchor);
                drawnShape.transform.parent = pumpkin.transform;
                drawnShape.transform.localPosition = anchorPosition;
                drawnShape.SetPoints(points);

                yield return null;
            }

            //remove the original anchor
            Destroy(drawnShapeAnchor);

            //Make pumpkin face user
            var lookDirection = Camera.main.transform.position - pumpkin.transform.position;
            lookDirection.y = 0f;
            pumpkin.transform.rotation = Quaternion.LookRotation(-lookDirection);

            pumpkin.SetActive(true);
        }
    }
}