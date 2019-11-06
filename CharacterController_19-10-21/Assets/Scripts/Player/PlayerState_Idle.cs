using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThirdPersonCharaterController;

public class PlayerState_Idle : BasePlayerState
{
    private CharacterMotionController _motioner = null;

    public override string GetName()
    {
        return PlayerStateMachine.PLATER_STATE_IDLE;
    }

    public override void Init(object parameters)
    {
        base.Init(parameters);

        _motioner = stateMachine.motionController;
    }

    public override void OnDestroy()
    {
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

        if (CharacterInputController.Instance.IsMoveInputed)
        {
            if (stateMachine != null)
            {
                stateMachine.ChangeState(PlayerStateMachine.PLATER_STATE_WALK,null);
            }
        }
    }

    public override void UnInit()
    {
        
    }
}
