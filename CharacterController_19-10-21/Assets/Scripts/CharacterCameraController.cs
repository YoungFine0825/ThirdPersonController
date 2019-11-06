using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPersonCharaterController
{
    public class CharacterCameraController : MonoBehaviour
    {
        public Transform focus;

        public CharacterMotionController actMotionController;

        public float Distance = 5.0f;

        public float Height = 2.0f;

        public bool EnableUpdate = true;

        public bool IsDebug = false;

        public Vector3 lookDirection { get; private set; }

        private float _yRotation = 0;

        // Start is called before the first frame update
        void Awake()
        {
            if (focus != null)
            {
                lookDirection = focus.forward;
            }
            else
            {
                lookDirection = transform.forward;
            }

            OnAwake();
        }

        private void Start()
        {
            OnStart();
        }

        private void OnDestroy()
        {

        }

        private void LateUpdate()
        {
            if(!EnableUpdate)
            {
                return;
            }

            if (actMotionController == null)
            {
                return;
            }

            lookDirection = Quaternion.AngleAxis(CharacterInputController.Instance.current.LookInput.x, actMotionController.up) * lookDirection;

            transform.position = focus.position;

            _yRotation += CharacterInputController.Instance.current.LookInput.y;

            Vector3 left = Vector3.Cross(lookDirection, actMotionController.up);

            transform.rotation = Quaternion.LookRotation(lookDirection, actMotionController.up);
            transform.rotation = Quaternion.AngleAxis(_yRotation, left) * transform.rotation;


            transform.position -= transform.forward * Distance;
            transform.position += actMotionController.up * Height;

        }

        protected virtual void OnAwake()
        {

        }

        protected virtual void OnStart()
        {

        }

    }
}
