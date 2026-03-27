using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheBlockWar.Scripts.Games.Turns;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Players;
[GlobalClass]
public partial class StatePlayer : Node, IPlayer
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public GameScheduler? GameScheduler { get; set; }
    public IReadOnlyList<SkillState> SkillingUnitSkillStates => _skillStates;
    public int Power { get; set => SetField(ref field, value); }
    public bool IsTurn { get; set => SetField(ref field, value); }
    public Vector2I CurrentMouseCoords { get; private set; }
    public TurnManager TurnManager { get; set; }
    public long SelectedUnitId { get; set => SetField(ref field, value); }
    
    private GDTaskCompletionSource _completionSource = new();
    private StateMachine<State> _stateMachine;
    private FreeState _freeState;
    private SkillingState _skillingState;
    private SkillState[] _skillStates = [];
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public StatePlayer()
    {
        _freeState = new(this);
        _skillingState = new(this);
        _stateMachine = new StateMachine<State>(_freeState);
    }


    public void OnInput<T>(in T input)
    {
        Logger.Trace("玩家{player}获取到输入{input}", GetHashCode(), input);
        if (input is PlayerInputMouseCoords coords)
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
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}