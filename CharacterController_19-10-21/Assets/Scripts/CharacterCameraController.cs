using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPersonCharaterController
{
    public class CharacterCameraController : MonoBehaviour
    {

        public Transform focus;

        public float Distance = 5.0f;

        public float Height = 2.0f;

        public Vector3 lookDirection { get; private set; }

        private CharacterMotionController _actMotionController;

        private float _yRotation = 0;

        // Start is called before the first frame update
        void Awake()
        {
            if (focus != null)
            {
                _actMotionController = focus.GetComponent<CharacterMotionController>();

                lookDirection = focus.forward;
            }
        }

        private void LateUpdate()
        {
           
            lookDirection = Quaternion.AngleAxis(CharacterInputController.Instance.current.LookInput.x, _actMotionController.up) * lookDirection;

            transform.position = focus.position;

            DebugDrawer.DrawMarker(transform.position, 2, Color.black, 0);

            _yRotation += CharacterInputController.Instance.current.LookInput.y;

            Vector3 left = Vector3.Cross(lookDirection, _actMotionController.up);

            transform.rotation = Quaternion.LookRotation(lookDirection, _actMotionController.up);
            transform.rotation = Quaternion.AngleAxis(_yRotation, left) * transform.rotation;

            DebugDrawer.DrawMarker(transform.position, 2, Color.red, 0);

            transform.position -= transform.forward * Distance;
            transform.position += _actMotionController.up * Height;

            DebugDrawer.DrawMarker(transform.position, 2, Color.green, 0);

            //DebugDrawer.DrawVector(transform.position,transform.forward,4,2,Color.cyan,0);
        }

        //private void OnDrawGizmos()
        //{
        //    if (focus != null)
        //    {
        //        Vector3 focusPos = focus.position;
        //        focusPos.y += Height;
        //        Gizmos.DrawWireSphere(focusPos, Distance);
        //    }

        //    Gizmos.DrawLine(transform.position, transform.forward);
        //}
    }
}
