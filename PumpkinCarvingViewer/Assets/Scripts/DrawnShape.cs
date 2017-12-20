using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityUtils;

namespace JI.Unity.PumpkinCarver.Viewer
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
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
        private MeshRenderer MeshRenderer { get; set; }

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
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}