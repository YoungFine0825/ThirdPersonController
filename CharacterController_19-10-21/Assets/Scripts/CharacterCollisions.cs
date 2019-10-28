using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThirdPersonCharaterController
{

    public class CharacterCollisions
    {
        public static bool ClosestPointOnSurface(Collider col, Vector3 to, float radius,out Vector3 concatPoint)
        {
            bool isConcated = true;

            if (col is BoxCollider)
            {
                concatPoint = ClosestOnOBB((BoxCollider)col, to);
            }
            else if (col is SphereCollider)
            {
                concatPoint = col.ClosestPointOnBounds(to);
            }
            else if (col is MeshCollider)
            {
                BSPTree bsp = col.gameObject.GetComponent<BSPTree>();
                if (bsp != null)
                {
                    concatPoint = bsp.ClosestPointOn(to,radius);
                }
                else
                {
                    concatPoint = col.ClosestPointOnBounds(to);
                }
                
            }
            else
            {
                concatPoint = Vector3.zero;
                isConcated = false;
            }

            return isConcated;
        }



        private static Vector3 ClosestOnOBB(BoxCollider col, Vector3 to)
        {
            Vector3 ret = Vector3.zero;

            Transform ct = col.transform;

            Vector3 localTo = ct.InverseTransformPoint(to);

            localTo -= col.center;

            Vector3 localNorm = new Vector3(
                Mathf.Clamp(localTo.x, -col.size.x * 0.5f, col.size.x * 0.5f),
                Mathf.Clamp(localTo.y, -col.size.y * 0.5f, col.size.y * 0.5f),
                Mathf.Clamp(localTo.z, -col.size.z * 0.5f, col.size.z * 0.5f)
                );

            localNorm += col.center;

            ret = ct.TransformPoint(localNorm);

            return ret;
        }
    }
}
