using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[SerializeField]
public class CollisionSphere
{
    public bool isHead = false;
    public bool isFeet = false;
    public float offset = 0;
    public float radius = 0.5f;
    public CollisionSphere()
    {

    }

    public CollisionSphere(bool isHead, bool isFeet, float Offset,float radius = 0.5f)
    {
        this.isHead = isHead;
        this.isFeet = isFeet;
        this.offset = Offset;
        this.radius = radius;
    }
}

public class CharacterController : MonoBehaviour
{

    public Transform target;

    [SerializeField]
    private CollisionSphere[] spheres = new CollisionSphere[3]{
        new CollisionSphere(false,true,-0.5f),
        new CollisionSphere(false,false,0),
        new CollisionSphere(true,false,0.5f)
    };

    private bool _onContacted = false;

    private Vector3 _contactPoint = Vector3.zero;

    private const float TinyTolerance = 0.01f;
    private const string TemporaryLayer = "TempCast";
    private int TemporaryLayerIndex;

    private void Awake()
    {
        TemporaryLayerIndex = LayerMask.NameToLayer(TemporaryLayer);
    }

    void LateUpdate()
    {
        PushBack();
    }

    private void PushBack()
    {
        if (spheres != null)
        {
            _onContacted = false;

            _contactPoint = Vector3.zero;

            for (int i = 0; i < spheres.Length; i++)
            {
                CollisionSphere s = spheres[i];
                
                Vector3 spherePosition = SpherePosition(s);

                float sphereRadius = s.radius;
                
                bool isConcatedSucceeded = false;

                if (s != null)
                {
                    foreach (Collider col in Physics.OverlapSphere( spherePosition, sphereRadius ) )
                    {
                        isConcatedSucceeded = CharacterCollisions.ClosestPointOnSurface(col, spherePosition,out _contactPoint);

                        if (isConcatedSucceeded)
                        {
                            DebugDrawer.DrawMarker(_contactPoint, 5, Color.blue, 0);

                            Vector3 v = _contactPoint - spherePosition;

                            // 先保存被碰撞对象的层Id
                            int layer = col.gameObject.layer;

                            //通过设置临时Layer，将除当前被碰撞体以外的物体忽略掉。
                            col.gameObject.layer = TemporaryLayerIndex;

                            // 从CollisionSphere的中点向接触点的方向发射一条射线，检测CollisionSphere的中点是否在被碰撞体的内部。
                            bool facingNormal = Physics.SphereCast(new Ray(spherePosition, v.normalized), TinyTolerance, v.magnitude + TinyTolerance, 1 << TemporaryLayerIndex);

                            col.gameObject.layer = layer;

                            if (facingNormal)
                            {
                                //CollisionSphere在被碰撞体的外部
                                if (Vector3.Distance(spherePosition, _contactPoint) < sphereRadius)
                                {
                                    //CollisionSphere的半径减去向量的模得到反推的距离，然后反转得到反推的向量
                                    v = v.normalized * (sphereRadius - v.magnitude) * -1;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                //CollisionSphere在被碰撞体的内部
                                v = v.normalized * (sphereRadius + v.magnitude);
                            }

                            target.position += v;

                            
                        }
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {

        if (spheres != null)
        {
            for (int i = 0; i < spheres.Length; ++i)
            {
                Gizmos.color = spheres[i].isFeet ? Color.green : (spheres[i].isHead ? Color.yellow : Color.cyan);
                Gizmos.DrawWireSphere(SpherePosition(spheres[i]), spheres[i].radius);
                if (_onContacted)
                {
                    Gizmos.DrawLine(SpherePosition(spheres[i]), SpherePosition(spheres[i]) - _contactPoint);
                }
                
            }
        }

    }

    private Vector3 SpherePosition(CollisionSphere sphere)
    {
        Vector3 ret = Vector3.zero;
        if (target != null)
        {
            ret = target.position;
        }
        else
        {
            ret = transform.position;
        }

        ret.y += sphere.offset;

        return ret;
    }
}
