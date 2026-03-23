namespace TheBlockWar.Scripts.Tools;

public class StateMachine<T> where T : IState
{
    public StateMachine(T initialState)
    {
        PreviousState = initialState;
        CurrentState = initialState;
    }

    public T CurrentState
    {
        get;
        private set
        {
            field = value;
            CurrentStateChanged?.Invoke(this, value);
        }
    }

    public EventHandler<T>? CurrentStateChanged;
    public T PreviousState { get; private set; }

    public void Init()
    {
        CurrentState.OnEnter();
    }
    public void ChangeState(T newState)
    {
        CurrentState.OnExit();
        PreviousState = CurrentState;
        CurrentState = newState;
        CurrentState.OnEnter();
    }

    public void ResetState()
    {
        ChangeState(CurrentState);
    }

    public void BackPrevState()
    {
        ChangeState(PreviousState);
    }
}