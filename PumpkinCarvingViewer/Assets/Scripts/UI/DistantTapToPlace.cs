using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BM.NYCC.Demo.UI
{
    /// <summary>
    /// Tap to place an object from a distance so that it faces towards the user, rather than
    /// as if the user is behind the object. This also assumes a constant offset instead of
    /// interpolating against a collider like in the MixedReality Toolkit
    /// </summary>
    public class DistantTapToPlace : MonoBehaviour, IInputClickHandler
    {
        [Tooltip("Whether or not this object should turn to face the user while being placed")]
        public bool turnToFaceUser = true;

        private bool isBeingPlaced;

        /// <summary>
        /// Offset from the point of collision to actually place the mesh
        /// </summary>
        private Vector3 offset = Vector3.zero;
        private Transform User { get; set; }

        private readonly RaycastHit[] _RaycastHitBuffer = new RaycastHit[4];
        private int _raycastBufferSize;
        private RaycastHit[] RaycastHitBuffer { get { return _RaycastHitBuffer; } }


        // Use this for initialization
        void Start()
        {
            User = Camera.main.transform;

            //For rapid development and simplicity, just use the AABB of the collider
            var collider = GetComponent<Collider>();

            if(collider != null)
            {
                offset = Vector3.zero;
                offset.y = transform.position.y - collider.bounds.min.y;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!isBeingPlaced)
                return;

            if (SpatialMappingManager.Instance != null)
            {
                var origin = User.position;
                var direction = User.forward;

                if (Physics.RaycastNonAlloc(origin, direction, RaycastHitBuffer, 20f, SpatialMappingManager.Instance.LayerMask) > 0)
                {
                    //Just assume the first collision is the most useful one
                    var hitInfo = RaycastHitBuffer[0];

                    transform.position = hitInfo.point + offset;

                    if (turnToFaceUser)
                    {
                        var target = User.position;
                        target.y = transform.position.y;

                        transform.LookAt(target);
                    }                 
                }
            }
        }
        public virtual void OnInputClicked(InputClickedEventData eventData)
        {
            isBeingPlaced = !isBeingPlaced;

            if (isBeingPlaced)
                RemoveWorldAnchor();
            else
                AttachWorldAnchor();
        }

        private void AttachWorldAnchor()
        {
            if (WorldAnchorManager.Instance != null)
            {
                // Add world anchor when object placement is done.
                WorldAnchorManager.Instance.AttachAnchor(gameObject);
            }
        }

        private void RemoveWorldAnchor()
        {
            if (WorldAnchorManager.Instance != null)
            {
                WorldAnchorManager.Instance.RemoveAnchor(gameObject);
            }
        }
    }
}