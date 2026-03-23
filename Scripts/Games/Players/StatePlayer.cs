using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Players;
[GlobalClass]
public partial class StatePlayer : Node, IPlayer
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public GameScheduler? GameScheduler { get; set; }
    public Vector2I CurrentMouseCoords { get; private set; }
    public GDTask Complete => _completionSource.Task;
    private GDTaskCompletionSource _completionSource = new();
    private StateMachine<State> _stateMachine;
    private FreeState _freeState;
    private SkillState _skillState;
    public StatePlayer()
    {
        _freeState = new(this);
        _skillState = new(this);
        _stateMachine = new StateMachine<State>(_freeState);
    }


    public void OnInput<T>(in T input)
    {
        Logger.Trace("玩家{player}获取到输入{input}", GetHashCode(), input);
        if (input is PlayerInputMousceCoords coords)
        {
            CurrentMouseCoords = coords.Coords;
        }
        _stateMachine.CurrentState.OnInput(input);
    }

    public void Init()
    {
        _stateMachine.Init();
    }
    public void CompleteTurn()
    {
        _completionSource.TrySetResult();
        _completionSource = new();
    }
}