using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCollisions 
{

    public static Vector3 ClosestPointOnSurface(SphereCollider col, Vector3 to)
    {
        Vector3 p = Vector3.zero;

        p = to - col.transform.position;

        p.Normalize();

        p *= col.radius * col.transform.localScale.x;

        p += col.transform.position;

        return p;
    }


    public static Vector3 ClosestPointOnSurface(BoxCollider col, Vector3 to)
    {
        if (col.transform.rotation == Quaternion.identity)
        {
            return col.ClosestPointOnBounds(to);
        }
        else
        {
            return ClosestOnOBB(col, to);
        }
    }


    public static bool ClosestPointOnSurface(Collider col, Vector3 to,out Vector3 concatPoint)
    {
        bool isConcated = true;

        if (col is BoxCollider)
        {
            concatPoint = CharacterCollisions.ClosestPointOnSurface((BoxCollider)col, to);
        }
        else if (col is SphereCollider)
        {
            concatPoint = CharacterCollisions.ClosestPointOnSurface((SphereCollider)col, to);
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
