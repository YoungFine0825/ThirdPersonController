﻿using System.Collections;
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
        private CharacterController _controller;
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
            CharacterController controller, 
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

            //用于碰撞检测的Sphere的最小半径
            float smallerRadius = _controller.radius - (_tolerance * _tolerance);

            RaycastHit hit;

            //向正下方投射Sphere看是是否碰撞到地面了
            if (Physics.SphereCast(o, smallerRadius,down, out hit, Mathf.Infinity, _walkableLayer, _triggerInteraction))
            {
                //拿到地面物体的碰撞属性
                GroundCollisionAttribute collisionAttr = hit.collider.GetComponent<GroundCollisionAttribute>();
                if (collisionAttr == null)
                {
                    collisionAttr = hit.collider.gameObject.AddComponent<GroundCollisionAttribute>();
                }
                collisionAttribute = collisionAttr;

                transform = hit.transform;

                //检查是否是踩到斜坡上
                SimulateSphereCast(hit.normal, out hit);

                _primaryGround = new GroundHit(hit.point, hit.normal, hit.distance);

                //如果碰撞点离控制器位置的距离足够小
                if (Vector3.Distance(Math3d.ProjectPointOnPlane(_controller.up, _controller.worldPosition, hit.point), _controller.worldPosition) < _tinyTolerance)
                {
                    return;
                }

                Vector3 toCenter = Math3d.ProjectVectorOnPlane(up, (_controller.worldPosition - hit.point).normalized * _tinyTolerance);


            }


        }


        private bool SimulateSphereCast(Vector3 groundNormal, out RaycastHit hit)
        {
            float groundAngle = Vector3.Angle(groundNormal, _controller.up) * Mathf.Deg2Rad;

            Vector3 secondaryOrigin = _controller.worldPosition + _controller.up * _tolerance;

            if (!Mathf.Approximately(groundAngle, 0))
            {
                float hor = Mathf.Sin(groundAngle) * _controller.radius;
                float ver = (1 - Mathf.Cos(groundAngle)) * _controller.radius;

                //计算指向斜坡的向量
                Vector3 r2 = Vector3.Cross(groundNormal,_controller.down);
                Vector3 v2 = Vector3.Cross(r2,groundNormal);

                //将指向斜坡的向量投射到控制器所在的平面（控制器的正上方向量即为该平面的法向量）
                Vector3 v3 = v2 - (Vector3.Dot(v2, _controller.up) * _controller.up);

                //计算新的起始点
                secondaryOrigin += v3.normalized * hor + _controller.up * ver;
            }

            //向正下方发射射线检测是否碰撞地面
            if (Physics.Raycast(secondaryOrigin, _controller.down, out hit, Mathf.Infinity, _walkableLayer, _triggerInteraction))
            {
                hit.distance -= _tolerance + _tinyTolerance;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}