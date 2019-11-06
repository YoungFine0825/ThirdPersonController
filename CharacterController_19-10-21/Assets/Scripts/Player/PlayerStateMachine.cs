using ThirdPersonCharaterController;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public const string PLATER_STATE_IDLE = "PLATER_STATE_IDLE";
    public const string PLATER_STATE_WALK = "PLATER_STATE_WALK";
    public const string PLATER_STATE_FALL = "PLATER_STATE_FALL";

    public CharacterMotionController motionController;

    public PlayerMotionAttribute motionAttribute = null;

    public string curStateName = string.Empty;

    public IPlayerState curState
    {
        get
        {
            if (_stateMachine == null)
            {
                return null;
            }

            ICommonState curState = _stateMachine.curState;
            if (curState != null)
            {
                return (IPlayerState)curState;
            }
            else
            {
                return null;
            }
        }
    }

    private CommonStateMachine _stateMachine;

    private void Awake()
    {
        _stateMachine = new CommonStateMachine();
        _stateMachine.Init();

        if (motionAttribute == null)
        {
            motionAttribute = this.GetComponent<PlayerMotionAttribute>();
        }


    }

    private void Start()
    {
        _stateMachine.RegisterState(new PlayerState_Idle(), this);
        _stateMachine.RegisterState(new PlayerState_Walk(), this);
        _stateMachine.RegisterState(new PlayerState_Fall(), this);
        _stateMachine.ChangeState(PLATER_STATE_IDLE,null);
    }

    private void Update()
    {
        if (curState != null)
        {
            curState.OnUpdate();
        }
    }

    private void LateUpdate()
    {
        if (curState != null)
        {
            curState.OnLateUpdate();
        }
    }

    private void OnDestroy()
    {
        if (_stateMachine != null)
        {
            _stateMachine.UnInit();
            _stateMachine = null;
        }
    }


    public void ChangeState(string stateName, object parameters)
    {
        if (_stateMachine != null)
        {
            if (_stateMachine.ChangeState(stateName, parameters) == 0)
            {
                curStateName = stateName;
            }
        }
    }

    public IPlayerState GetState(string key)
    {
        if (_stateMachine != null)
        {
            ICommonState state = _stateMachine.GetState(key);

            if (state != null)
            {
                return (IPlayerState)state;
            }

        }

        return null;
    }
}
