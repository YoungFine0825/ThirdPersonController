using DG.Tweening;
using UnityEngine;

namespace ThirdPersonCharaterController
{



    [SerializeField]
    public class CollisionSphere
    {
        public bool isHead = false;
        public bool isFeet = false;
        public float offset = 0;
        public float radius = 0.5f;

        public Vector3 localPosition {
            get
            {
                if (_controller != null)
                {
                    Vector3 ret = _controller.worldPosition;
                    ret.y += offset;
                    return ret;
                }
                else
                {
                    return new Vector3(0,offset,0);
                }
            }
        }

        private CharacterMotionController _controller;

        public CollisionSphere()
        {

        }

        public CollisionSphere(bool isHead, bool isFeet, float Offset, float radius = 0.5f)
        {
            this.isHead = isHead;
            this.isFeet = isFeet;
            this.offset = Offset;
            this.radius = radius;
        }

        public void AttachController(CharacterMotionController controller)
        {
            _controller = controller;
        }
    }

    public class CharacterMotionController : MonoBehaviour
    {
        
        public float radius = 0.5f;

        [SerializeField]
        QueryTriggerInteraction triggerInteraction;

        public LayerMask Walkable;

        public bool IsClampToGround = true;

        public float ClmapDistance = 1f;

        public bool IsSlopeLimit = true;

        public CharacterCameraController _cameraController;

        public bool IsDebug = true;

        public bool IsDebugGroundCollision = false;

        public Vector3 up {
            get
            {
                return transform.up;
            }
        }

        public Vector3 down
        {
            get
            {
                return -transform.up;
            }
        }

        public Vector3 worldPosition {
            get
            {
                return transform.position;
            }
        }

        public GroundCollider currentGround { get; private set; }

        public CollisionSphere head { get; private set; }

        public CollisionSphere feet { get; private set; }

        [SerializeField]
        public CollisionSphere[] spheres = new CollisionSphere[3]{
        new CollisionSphere(false,true,0.5f),
        new CollisionSphere(false,false,1.0f),
        new CollisionSphere(true,false,1.5f)
        };

        private bool _onContacted = false;

        private Vector3 _contactPoint = Vector3.zero;
        private const float Tolerance = 0.05f;
        private const float TinyTolerance = 0.01f;
        private const string TemporaryLayer = "TempCast";
        private int TemporaryLayerIndex;

        private Vector3 _movementDirection = Vector3.zero;
        private Vector3 _movementDelta = Vector3.zero;

        private Vector3 _posBeforeCorrection;

        private float _rotationAngle = 0;
        private Vector3 _lastLookDir;

        private void Awake()
        {
            TemporaryLayerIndex = LayerMask.NameToLayer(TemporaryLayer);

            currentGround = new GroundCollider(Walkable, this, triggerInteraction, Tolerance, TinyTolerance);

            foreach (CollisionSphere sphere in spheres)
            {
                sphere.AttachController(this);

                if (sphere.isFeet)
                    feet = sphere;

                if (sphere.isHead)
                    head = sphere;
            }

            if (feet == null)
                Debug.LogError("[SuperCharacterController] Feet not found on controller");

            if (head == null)
                Debug.LogError("[SuperCharacterController] Head not found on controller");

            _lastLookDir = Vector3.forward;
        }


        void Update()
        {

            if (CharacterInputController.Instance.current.MoveInput != Vector3.zero)
            {
                Movement();
            }
            

            CorrectingPosition();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="moveInput"></param>
        /// <param name="speed"></param>
        public void Move(Vector3 direction,Vector3 moveInput,float speed)
        {
            _movementDirection = direction;

            Vector3 moveDir = Vector3.zero;

            if (moveInput.x != 0)
            {
                moveDir += -Vector3.Cross(direction, up) * moveInput.x;
            }

            if (moveInput.z != 0)
            {
                moveDir += direction * moveInput.z;
            }

            _movementDelta = moveDir.normalized * speed * Time.deltaTime;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="speed"></param>
        public void SimpleMove(Vector3 speed)
        {
            _movementDelta = speed;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Movement()
        {
            Vector3 dir = _cameraController.lookDirection;
            

            Vector3 moveInput = CharacterInputController.Instance.current.MoveInput;
            Vector3 moveDir = Vector3.zero;

            if (moveInput.x != 0)
            {
                moveDir += -Vector3.Cross(dir, up) * moveInput.x;
            }
            if (moveInput.z != 0)
            {
                moveDir += dir * moveInput.z;
            }

            
            _rotationAngle += Vector3.SignedAngle(_lastLookDir,moveDir.normalized,up);
            transform.DORotateQuaternion(Quaternion.AngleAxis(_rotationAngle,up), 0.5f);

            transform.position +=  moveDir.normalized * (5 * Time.deltaTime);

            _lastLookDir = moveDir;
        }



        /// <summary>
        /// 
        /// </summary>
        private void CorrectingPosition()
        {
            
            _posBeforeCorrection = transform.position;

            currentGround.ProbeGround(SpherePosition(feet), 1);

            PushBack(1, 1);

            currentGround.ProbeGround(SpherePosition(feet), 2);

            if (IsSlopeLimit)
            {
                SlopeLimit();
            }

            currentGround.ProbeGround(SpherePosition(feet), 3);

            if (IsClampToGround)
            {
                ClmapToGround();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="iter"></param>
        /// <param name="maxIter"></param>
        private void PushBack(int iter,int maxIter)
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
                        foreach (Collider col in Physics.OverlapSphere(spherePosition, sphereRadius))
                        {
                            isConcatedSucceeded = CharacterCollisions.ClosestPointOnSurface(col, spherePosition,sphereRadius, out _contactPoint);

                            if (isConcatedSucceeded)
                            {
                                DebugDrawer.DrawMarker(_contactPoint, 1, Color.red, 0);

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

                                transform.position += v;

                                _onContacted = true;

                            }
                        }
                    }
                }
            }

            if (iter < maxIter && _onContacted)
            {
                PushBack(++iter, maxIter);
            }
        }

        private void OnDrawGizmos()
        {
            if (!IsDebug)
            {
                return;
            }
            if (spheres != null)
            {
                for (int i = 0; i < spheres.Length; ++i)
                {
                    Gizmos.color = spheres[i].isFeet ? Color.green : (spheres[i].isHead ? Color.yellow : Color.cyan);
                    Gizmos.DrawWireSphere(SpherePosition(spheres[i]), spheres[i].radius);
                }
            }

            if (IsDebugGroundCollision && currentGround != null)
            {
                currentGround.DebugGroundHit();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sphere"></param>
        /// <returns></returns>
        private Vector3 SpherePosition(CollisionSphere sphere)
        {
            Vector3 ret = transform.position;

            ret.y += sphere.offset;

            return ret;
        }


        /// <summary>
        /// 减去与当前地面的距离，已达到紧贴地面的效果
        /// </summary>
        private void ClmapToGround()
        {
            float dis = currentGround.GetDistance();
            if (dis <= ClmapDistance)
            {
                transform.position -= up * dis;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="enable"></param>
        public void EnableClmapToGround(bool enable)
        {
            IsClampToGround = enable;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool SlopeLimit()
        {
            Vector3 groundNormal = currentGround.GetNormal();
            float angle = Vector3.Angle(groundNormal,up);

            float standAngle = GroundCollisionAttribute.DEFAULT_STAND_ANGLE;
            float slopeLimit = GroundCollisionAttribute.DEFAULT_SLOPE_LIMIT;

            if (currentGround.collisionAttribute != null)
            {
                standAngle = currentGround.collisionAttribute.StandAngle;
                slopeLimit = currentGround.collisionAttribute.SlopeLimit;
            }

            if (angle >= slopeLimit)
            {
                Vector3 absoluteMoveDirection = Math3d.ProjectVectorOnPlane(groundNormal, transform.position - _posBeforeCorrection);

                // Retrieve a vector pointing down the slope
                Vector3 r = Vector3.Cross(groundNormal, down);
                Vector3 v = Vector3.Cross(r, groundNormal);

                float absoluteAngle = Vector3.Angle(absoluteMoveDirection, v);

                if (absoluteAngle <= 90.0f)
                {
                    return false;
                }

                // Calculate where to place the controller on the slope, or at the bottom, based on the desired movement distance
                Vector3 resolvedPosition = Math3d.ProjectPointOnLine(_posBeforeCorrection, r, transform.position);
                Vector3 direction = Math3d.ProjectVectorOnPlane(groundNormal, resolvedPosition - transform.position);

                RaycastHit hit;

                // Check if our path to our resolved position is blocked by any colliders
                if (Physics.CapsuleCast(SpherePosition(feet), SpherePosition(head), radius, direction.normalized, out hit, direction.magnitude, Walkable, triggerInteraction))
                {
                    transform.position += v.normalized * hit.distance;
                }
                else
                {
                    transform.position += direction;
                }

                return true;
            }
            return false;
        }
    }

}
