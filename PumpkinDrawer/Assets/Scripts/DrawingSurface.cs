using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JI.Unity.PumpkinCarver.Drawer
{
    [RequireComponent(typeof(LineRenderer))]
    public class DrawingSurface : MonoBehaviour
    {
        private enum State
        {
            NO_ACTIVITY, DRAWING_LINE
        }

        [Tooltip("The minimum distance between points that are added to each path")]
        public float pointThreshold = 0.01f;

        [Tooltip("Instantiate a drawn shape when a new shape is drawn")]
        public DrawnShape drawnShapePrefab;

        [Tooltip("Where to place all the rendered shapes!")]
        public Transform shapeCollection;

        private LineRenderer LineRenderer { get; set; }
        private State state;

        // Use this for initialization
        void Start()
        {
            LineRenderer = GetComponent<LineRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            switch (state)
            {
                case State.NO_ACTIVITY:
                    if (IsTouchMoving() || IsMouseDown())
                    {
                        //See if we don't hit any buttons or shapes
                        var pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                        if (!Physics.Raycast(pointerRay))
                            StartLine();
                    }
                    break;
                case State.DRAWING_LINE:
                    if (IsTouchMoving() || IsMouseDown())
                    {
                        var pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                        var point = pointerRay.origin;

                        var n = LineRenderer.positionCount;

                        if (LineRenderer.positionCount == 0 || Vector3.Distance(point, LineRenderer.GetPosition(n - 1)) > pointThreshold)
                        {
                            LineRenderer.positionCount++;
                            LineRenderer.SetPosition(n, point);
                        }
                    }
                    else
                    {
                        SealLine();
                    }
                    break;
                default:
                    return;
            }
        }

        void StartLine()
        {
            state = State.DRAWING_LINE;
            LineRenderer.positionCount = 0;
        }

        void SealLine()
        {
            state = State.NO_ACTIVITY;

            if (LineRenderer.positionCount > 3)
            {
                var drawnShape = Instantiate(drawnShapePrefab);

                drawnShape.transform.parent = shapeCollection;
                drawnShape.transform.position = transform.position;

                var positionBuffer = new Vector3[LineRenderer.positionCount];
                LineRenderer.GetPositions(positionBuffer);

                //Sort all points for winding. Since 
                //we can assume that this line is constant flow, we will just determine
                //the winding and reverse the array if incorrect
                Vector3 center = Vector3.zero;
                for(var i = 0; i < positionBuffer.Length; ++i)
                {
                    center += positionBuffer[i];
                }

                center /= positionBuffer.Length;

                float det = 0;
                for(var i = 1; i < positionBuffer.Length; ++i)
                {
                    var a = positionBuffer[i - 1];
                    var b = positionBuffer[i];
                    det = (a.x - center.x) * (b.y - center.y) - (b.x - center.x) * (a.y - center.y);

                    if (det != 0f)
                        break;
                }

                if (det > 0f)
                    positionBuffer = positionBuffer.Reverse().ToArray();

                drawnShape.SetPoints(positionBuffer);
            }

            LineRenderer.positionCount = 0;
        }

        bool IsTouchMoving()
        {
            return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved;
        }

        bool IsMouseDown()
        {
            return Input.GetMouseButton(0);
        }
    }
}