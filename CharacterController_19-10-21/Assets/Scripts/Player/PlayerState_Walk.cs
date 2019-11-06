using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThirdPersonCharaterController;

public class PlayerState_Walk : BasePlayerState
{

    private CharacterCameraController _camera = null;

    private CharacterInputController _inputer = null;

    private CharacterMotionController _motioner = null;

    public override string GetName()
    {
        return PlayerStateMachine.PLATER_STATE_WALK;
    }

    public override void Init(object parameters)
    {
        base.Init(parameters);

        _inputer = CharacterInputController.Instance;

        _motioner = this.stateMachine.motionController;

        _camera = _motioner.cameraController;
    }

    public override void OnDestroy()
    {
        _camera = null;

        _inputer = null;

        _motioner = null;

}

    public override void OnEnter(string previousState, object parameters)
    {
        
    }

    public override void OnExit(string nextState)
    {
        
    }

    public override void OnLateUpdate()
    {

    }

    public override void OnUpdate()
    {

        if (!_motioner.IsGrounded())
        {
            this.stateMachine.ChangeState(PlayerStateMachine.PLATER_STATE_FALL, null);
        }
        if (_inputer.IsMoveInputed)
        {
            float walkSpeed = stateMachine.motionAttribute.WalkSpeed;

            Vector3 lookDirection = _camera.lookDirection;

            Vector3 moveInput = _inputer.current.MoveInput;

            Vector3 moveDir = Vector3.zero;

            if (moveInput.x != 0)
            {
                moveDir += -Vector3.Cross(lookDirection, _motioner.up) * moveInput.x;
            }

            if (moveInput.z != 0)
            {
                moveDir += lookDirection * moveInput.z;
            }

            _motioner.Move(moveDir.normalized, walkSpeed);
        }
        else
        {
            this.stateMachine.ChangeState(PlayerStateMachine.PLATER_STATE_IDLE, null);

        } 

    }

    public override void UnInit()
    {
        
    }
}
