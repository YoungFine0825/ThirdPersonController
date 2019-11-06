
public abstract class BasePlayerState : IPlayerState
{
    protected PlayerStateMachine stateMachine;

    public abstract string GetName();
    public virtual void Init(object parameters)
    {
        this.stateMachine = (PlayerStateMachine)parameters;
    }
    public abstract void OnDestroy();
    public abstract void OnEnter(string previousState, object parameters);
    public abstract void OnExit(string nextState);
    public abstract void OnLateUpdate();
    public abstract void OnUpdate();
    public abstract void UnInit();
}
