using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityUtils;

namespace JI.Unity.PumpkinCarver.Drawer
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class DrawnShape : MonoBehaviour
    {
        [Tooltip("The material to render the shape with")]
        public Material material;

        private MeshFilter _MeshFilter;
        private MeshFilter MeshFilter
        {
            get
            {
                if (_MeshFilter == null)
                    _MeshFilter = GetComponent<MeshFilter>();

                return _MeshFilter;
            }
        }

        private MeshCollider _MeshCollider;
        private MeshCollider MeshCollider
        {
            get
            {
                if (_MeshCollider == null)
                    _MeshCollider = GetComponent<MeshCollider>();

                return _MeshCollider;
            }
        }
        private MeshRenderer MeshRenderer { get; set; }
        private Vector3 lastMousePosition;
        private Vector3 lastPosition;

        // Use this for initialization
        void Start()
        {
            MeshRenderer = GetComponent<MeshRenderer>();
            MeshRenderer.material = material;
        }

        public void SetPoints(IList<Vector3> points)
        {
            var mesh = new Mesh();
            //var vertices = new Vector3[points.Count + 1];

            //drop Z Coord
            var vertices = points.Select(point => new Vector2(point.x, point.y)).ToArray();
            var triangulator = new Triangulator(vertices);

            mesh.vertices = points.ToArray();
            mesh.triangles = triangulator.Triangulate();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            MeshFilter.mesh = mesh;
            MeshCollider.sharedMesh = mesh;
        }


        // Update is called once per frame
        void Update()
        {
            //if(IsTouchMoving() || IsMouseDown())
            //{
            //    var pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            //    RaycastHit hitInfo;
            //    if(MeshCollider.Raycast(pointerRay, out hitInfo, 20f))
            //    {
            //        var delta = Input.
            //        transform.Translate()
            //    }
            //}
            //Move along with the mouse
        }

        private void OnMouseDown()
        {
            var pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            lastMousePosition = pointerRay.origin;
            lastPosition = transform.position;
        }
        private void OnMouseDrag()
        {
            var pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            var d = pointerRay.origin - lastMousePosition;
            d.z = 0;
            transform.position = lastPosition + d;
        }
    }
}