using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPersonCharaterController
{
    public struct CharacterInput
    {
        public Vector3 MoveInput;
        public Vector3 LookInput;
        public bool IsJump;
    }

    public class CharacterInputController : MonoBehaviour
    {
        public static CharacterInputController Instance = null;

        public CharacterInput current;

        public Vector2 RightStickMultiplier = new Vector2(3, -1.5f);

        public bool IsMoveInputed
        {
            get
            {
                return current.MoveInput != Vector3.zero;
            }
        }

        public bool IsLookInputed
        {
            get
            {
                return current.LookInput != Vector3.zero;
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
            Instance = this;

            current = new CharacterInput();
        }

        // Update is called once per frame
        void LateUpdate()
        {

            current.MoveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

            current.LookInput = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            current.IsJump = Input.GetKeyDown(KeyCode.Space);

        }
    }
}
