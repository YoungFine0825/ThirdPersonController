using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPersonCharaterController
{
    public class GroundCollider
    {

        private class GroundHit
        {
            public Vector3 point { get; private set; }
            public Vector3 normal { get; private set; }
            public float distance { get; private set; }

            public GroundHit(Vector3 point, Vector3 normal, float distance)
            {
                this.point = point;
                this.normal = normal;
                this.distance = distance;
            }
        }


        private LayerMask _walkableLayer;
        private CharacterMotionController _controller;
        private QueryTriggerInteraction _triggerInteraction;

        private GroundHit _primaryGround;
        private GroundHit _nearGround;
        private GroundHit _farGround;
        private GroundHit _stepGround;
        private GroundHit _flushGround;

        public GroundCollisionAttribute collisionAttribute { get; private set; }
        public Transform transform { get; private set; }

        private const float groundingUpperBoundAngle = 60.0f;
        private const float groundingMaxPercentFromCenter = 0.85f;
        private const float groundingMinPercentFromcenter = 0.50f;

        private float _tolerance = 0.05f;
        private float _tinyTolerance = 0.01f;

        public GroundCollider(
            LayerMask walkable, 
            CharacterMotionController controller, 
            QueryTriggerInteraction triggerInteraction,
            float tolerance = 0.05f,
            float tinyTolerance = 0.01f)
        {
            this._walkableLayer = walkable;
            this._controller = controller;
            this._triggerInteraction = triggerInteraction;
            this._tolerance = tolerance;
            this._tinyTolerance = tinyTolerance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="iter"></param>
        public void ProbeGround(Vector3 origin, int iter)
        {
            //Reset
            _primaryGround = null;
            _nearGround = null;
            _farGround = null;
            _stepGround = null;
            _flushGround = null;

            Vector3 up = _controller.up;
            Vector3 down = _controller.down;

            //碰撞检测起始位置
            Vector3 o = origin + (up * _tolerance);

            //用于碰撞检测的Sphere要小一点
            float smallerRadius = _controller.radius - (_tolerance * _tolerance);

            RaycastHit hit;

            //向正下方投射Sphere看是是否碰撞到物体了
            if (Physics.SphereCast(o, smallerRadius, down, out hit, Mathf.Infinity, _walkableLayer, _triggerInteraction))
            {
                float standAngle = 0;
                float slopeLimit = 0;
                //拿到地面物体的碰撞属性
                collisionAttribute = hit.collider.GetComponent<GroundCollisionAttribute>();
                if (collisionAttribute != null)
                {
                    standAngle = collisionAttribute.StandAngle;
                    slopeLimit = collisionAttribute.SlopeLimit;
                }
                else
                {
                    standAngle = GroundCollisionAttribute.DEFAULT_STAND_ANGLE;
                    slopeLimit = GroundCollisionAttribute.DEFAULT_SLOPE_LIMIT;
                }

                if (_controller.IsDebugGroundCollision)
                {
                    DebugDrawer.DrawVector(hit.point, hit.normal, 2.5f, 1, Color.yellow, 0);
                }
                
                //transform = hit.transform;

                //检测碰撞到是不是真正的地面（因为通过投射Sphere检测到的碰撞物体的法线是一个差值，需要得到地面的实际碰撞信息）
                SimulateSphereCast(hit.normal, out hit);

                //_primaryGround = new GroundHit(hit.point, hit.normal, hit.distance);

                //将碰撞点投射到控制器位置所在的平面，如果里控制器位置的距离足够小,表示碰撞到的是一个平坦的平面
                if (Vector3.Distance(Math3d.ProjectPointOnPlane(_controller.up, _controller.worldPosition, hit.point), _controller.worldPosition) < _tinyTolerance)
                {
                    return;
                }
                else
                {
                    _primaryGround = new GroundHit(hit.point, hit.normal, hit.distance);
                    transform = hit.transform;
                }

                Vector3 toCenter = Math3d.ProjectVectorOnPlane(up, (_controller.worldPosition - hit.point).normalized * _tinyTolerance);

                Vector3 awayFromCenter = Quaternion.AngleAxis(-80.0f, Vector3.Cross(toCenter, _controller.up)) * -toCenter;

                Vector3 nearPoint = hit.point + toCenter + (_controller.up * _tolerance);

                Vector3 farPoint = hit.point + (awayFromCenter * 3);

                RaycastHit nearHit;
                RaycastHit farHit;

                Physics.Raycast(nearPoint, _controller.down, out nearHit, Mathf.Infinity, _walkableLayer, _triggerInteraction);
                Physics.Raycast(farPoint, _controller.down, out farHit, Mathf.Infinity, _walkableLayer, _triggerInteraction);

                _nearGround = new GroundHit(nearHit.point, nearHit.normal, nearHit.distance);
                _farGround = new GroundHit(farHit.point, farHit.normal, farHit.distance);

                if (Vector3.Angle(hit.normal, _controller.up) > standAngle)
                {//碰撞到的面是了一个陡坡或墙（碰撞面的法线与控制器正上方向的夹角大于设置的标准角度）
                    

                    //计算平行于碰撞表面且向下的向量
                    Vector3 r = Vector3.Cross(hit.normal,_controller.down);
                    Vector3 v = Vector3.Cross(r, hit.normal);

                    Vector3 flushOrigin = hit.point + hit.normal * _tolerance;

                    RaycastHit flushHit;

                    //沿着碰撞面向下发射射线检测碰撞的表面
                    if (Physics.Raycast(flushOrigin, v, out flushHit, Mathf.Infinity, _walkableLayer, _triggerInteraction))
                    {
                        RaycastHit forTruethfulNormal;
                        //校正法线
                        if (SimulateSphereCast(flushHit.normal, out forTruethfulNormal))
                        {
                            _flushGround = new GroundHit(forTruethfulNormal.point, forTruethfulNormal.normal, forTruethfulNormal.distance);
                        }
                    }
                }

            }
            else if (Physics.Raycast(o, down, out hit, Mathf.Infinity, _walkableLayer, _triggerInteraction))
            {
                //拿到地面物体的碰撞属性
                collisionAttribute = hit.collider.GetComponent<GroundCollisionAttribute>();

                RaycastHit sphereHit;

                if (SimulateSphereCast(hit.normal, out sphereHit))
                {
                    _primaryGround = new GroundHit(sphereHit.point, sphereHit.normal, sphereHit.distance);
                }
                else
                {
                    _primaryGround = new GroundHit(hit.point, hit.normal, hit.distance);
                }
            }
            else
            {
                Debug.LogError("Not found ground!");
            }



        }


        /// <summary>
        /// 校正SphereCast得到的法线（如cast的位置在几个面连接处，那么得到的法线是一个插值）
        /// </summary>
        /// <param name="groundNormal"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        private bool SimulateSphereCast(Vector3 groundNormal, out RaycastHit hit)
        {

            float groundAngle = Vector3.Angle(groundNormal, _controller.up) * Mathf.Deg2Rad;

            Vector3 secondaryOrigin = _controller.worldPosition + _controller.up * _tolerance;

            if (!Mathf.Approximately(groundAngle, 0))
            {
                float hor = Mathf.Sin(groundAngle) * _controller.radius;
                float ver = (1 - Mathf.Cos(groundAngle)) * _controller.radius;

                //计算指向斜坡的向量
                Vector3 r2 = Vector3.Cross(groundNormal, _controller.down);
                Vector3 v2 = -Vector3.Cross(r2, groundNormal);

                //将指向斜坡的向量投射到控制器所在的平面（控制器的正上方向量即为该平面的法向量）
                Vector3 v3 = v2 - (Vector3.Dot(v2, _controller.up) * _controller.up);

                //计算新的起始点
                secondaryOrigin += v3.normalized * hor + _controller.up * ver;
            }

            if (_controller.IsDebugGroundCollision)
            {
                DebugDrawer.DrawVector(secondaryOrigin, _controller.down, 3, 1, Color.blue, 0);
            }
            

            //向正下方发射射线检测(注意这里是发射的射线不是Sphere)得到实际地面的法线
            if (Physics.Raycast(secondaryOrigin, _controller.down, out hit, Mathf.Infinity, _walkableLayer, _triggerInteraction))
            {
                hit.distance -= _tolerance + _tinyTolerance;

                if (_controller.IsDebugGroundCollision)
                {
                    DebugDrawer.DrawVector(hit.point, hit.normal, 3, 1, Color.magenta, 0);
                }
                
                return true;
            }
            else
            {
                return false;
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool isGrounded()
        {
            return _primaryGround != null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public float GetDistance()
        {
            if (_primaryGround != null)
            {
                return _primaryGround.distance;
            }
            return 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNormal()
        {
            if (_primaryGround != null)
            {
                return _primaryGround.normal;
            }
            return Vector3.zero;
        }
    }
}
