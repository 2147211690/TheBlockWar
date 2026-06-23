using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheBlockWar.Scripts.Games.Turns;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;
using TheBlockWar.Scripts.Views.TileViews;

namespace TheBlockWar.Scripts.Games.Players;
[GlobalClass]
public partial class StatePlayer : Node2D, IPlayer
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public EventScheduler? GameScheduler { get; set; }
    public IReadOnlyList<SkillState> SkillingUnitSkillStates => _skillStates;
    public int Power { get; set => SetField(ref field, value); }
    public int Team { get; private set; }

    public bool IsTurn
    {
        get;
        set
        {
            if (!SetField(ref field, value)) return;
            if (field)
            {
                _turnCompletionSource = new GDTaskCompletionSource();
                _turnCompletionSource.Task.Forget();
            }
            else
            {
                _turnCompletionSource?.TrySetResult();
                _turnCompletionSource = null;
            }
        }
    }

    public int PhaseOrder { get; set; }
    public Vector2I CurrentMouseCoords { get; private set; }
    public TurnManager TurnManager { get; set; } = null!;
    public long SelectedUnitId { get; set => SetField(ref field, value); }
    public long FocusedUnitId { get; set => SetField(ref field, value); }
    public IGameView GameView { get; private set; } = null!;
    
    private GDTaskCompletionSource? _turnCompletionSource;
    private StateMachine<State> _stateMachine;
    private FreeState _freeState;
    private SkillingState _skillingState;
    private SkillState[] _skillStates = [];
    private Vector2I _currentCoords;
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected StatePlayer()
    {
        _freeState = new(this);
        _skillingState = new(this);
        _stateMachine = new StateMachine<State>(_freeState);
    }

    public static StatePlayer Create(int team, IGameView gameView)
    {
        var player = new StatePlayer();
        player.Team = team;
        player.GameView = gameView;
        return player;
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        if (GameView.IsRunning) return;
        switch (@event)
        {
            case InputEventMouseMotion e:
                var current = GameView.NodePositionToCoords(GetGlobalMousePosition());
                if (_currentCoords != current)
                {
                    _currentCoords = current;
                    OnInput(new PlayerInputMouseCoords(_currentCoords));
                }

                break;
            case InputEventMouseButton e:
                OnInput(new PlayerInputClick(_currentCoords, e.ButtonMask));
                break;
        }
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

    public GDTask WaitTurnComplete() => _turnCompletionSource?.Task ?? GDTask.CompletedTask;

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