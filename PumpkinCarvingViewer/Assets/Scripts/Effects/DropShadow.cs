using UnityEngine;

namespace BM.NYCC.Demo.Effects
{
    public class DropShadow : MonoBehaviour
    {
        [Tooltip("The maximum distance at which the shadow will display")]
        public float maxShadowDistance = 20f;
        private Transform Parent { get; set; }

        private readonly RaycastHit[] _RaycastHitBuffer = new RaycastHit[4];
        private RaycastHit[] RaycastHitBuffer { get { return _RaycastHitBuffer; } }

        private Vector3 InitialScale { get; set; }
        private Quaternion DefaultRotation { get; set; }

        private void Awake()
        {
            Parent = transform.parent;
            InitialScale = transform.localScale;
            DefaultRotation = transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            transform.rotation = DefaultRotation;

            if(Physics.RaycastNonAlloc(Parent.position, Vector3.down, RaycastHitBuffer, 20f, ~(1 << 9)) > 0)
            {
                var hit = RaycastHitBuffer[0];
                transform.position = hit.point;

                var distance = Vector3.Distance(hit.point, Parent.position);

                transform.localScale = Vector3.Lerp(InitialScale, Vector3.zero, Mathf.SmoothStep(0f, 1f, distance / maxShadowDistance));
            }
        }
    }
}