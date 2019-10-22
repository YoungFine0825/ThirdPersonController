using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{

    public float radius = 1f;

    private bool _onConcated = false;

    private Vector3 _concatPoint = Vector3.zero;


    void LateUpdate()
    {
        _onConcated = false;

        _concatPoint = Vector3.zero;

        bool isConcatedSucceeded = false;

        foreach (Collider col in Physics.OverlapSphere(transform.position, radius))
        {
            isConcatedSucceeded = CharacterCollisions.ClosestPointOnSurface(col, transform.position,out _concatPoint);

            if (isConcatedSucceeded)
            {
                Vector3 v = transform.position - _concatPoint;

                transform.position += Vector3.ClampMagnitude(v,Mathf.Clamp(radius - v.magnitude,0,radius));

                _onConcated = true;
            }

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _onConcated ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position,radius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_concatPoint,0.2f);
    }
}
