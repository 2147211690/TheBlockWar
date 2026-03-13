using Godot;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Player;
[GlobalClass]
public partial class StatePlayer : Node, IPlayer
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private StateMachine<State> _stateMachine;
    private FreeState _freeState;
    private SkillState _skillState;
    public GameScheduler? GameScheduler { get; set; }
    public StatePlayer()
    {
        _freeState = new(this);
        _skillState = new(this);
        _stateMachine = new StateMachine<State>(_freeState);
    }
    
    public void OnInput<T>(in T input)
    {
        Logger.Trace("玩家{player}获取到输入{input}", GetHashCode(), input);
        _stateMachine.CurrentState.OnInput(input);
    }

    public void Init()
    {
        _stateMachine.Init();
    }
}