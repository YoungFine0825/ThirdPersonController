using ThirdPersonCharaterController;
using UnityEngine;

public class PlayerState_Fall : BasePlayerState
{

    private CharacterMotionController _motioner = null;

    private PlayerMotionAttribute _montionAttr = null;

    private CharacterInputController _inputer = null;

    private CharacterCameraController _camera = null;

    private string _previousState = string.Empty;

    private Vector3 _fallDirection = Vector3.zero;

    public override void Init(object parameters)
    {
        base.Init(parameters);

        _motioner = this.stateMachine.motionController;

        _montionAttr = this.stateMachine.motionAttribute;

        _camera = _motioner.cameraController;

        _inputer = CharacterInputController.Instance;
    }

    public override void UnInit()
    {
        _motioner = null;

        _montionAttr = null;

        _inputer = null;

        _camera = null;
    }

    public override string GetName()
    {
        return PlayerStateMachine.PLATER_STATE_FALL;
    }

    public override void OnDestroy()
    {

    }

    public override void OnEnter(string previousState, object parameters)
    {
        _previousState = previousState;

        Vector3 moveInput = _inputer.current.MoveInput;

        Vector3 lookDirection = _camera.lookDirection;

        _fallDirection = Vector3.zero;

        if (moveInput.x != 0)
        {
            _fallDirection += Vector3.Cross(_motioner.up, lookDirection) * moveInput.x;
        }

        if (moveInput.z != 0)
        {
            _fallDirection += lookDirection * moveInput.z;
        }

        _fallDirection += _motioner.down;
    }

    public override void OnExit(string nextState)
    {

    }

    public override void OnLateUpdate()
    {

    }

    public override void OnUpdate()
    {
        if (_motioner.IsGrounded())
        {
            stateMachine.ChangeState(_previousState, null);
        }
        else
        {
            _motioner.Move(_fallDirection, _montionAttr.Gravity);
        }

    }
}
