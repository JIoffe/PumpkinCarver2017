using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JI.Unity.PumpkinCarver.Drawer
{
    public class Spin : MonoBehaviour
    {
        [Tooltip("Velocity to spin each second")]
        public Vector3 spinVelocity = Vector3.zero;
        // Update is called once per frame
        void Update()
        {
            transform.Rotate(spinVelocity * Time.deltaTime);
        }
    }
}