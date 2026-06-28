using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Turns;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;
using TheBlockWar.Scripts.Views.TileViews;

namespace TheBlockWar.Scripts.Games.Players;
[GlobalClass]
public partial class StatePlayer : Node2D, IPlayer
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public IEventScheduler? GameScheduler { get; set; }
    public int Power { get; set => SetField(ref field, value); }
    public int Team { get; set => SetField(ref field, value); }
    public bool IsTurn { get; set => SetField(ref field, value); }
    public PlayerTurnEnterEnum TurnEnter { get; set; }
    public ITurnManager TurnManager { get; set; }
    public int PhaseOrder { get; set => SetField(ref field, value); }
    public int CurrentTurn { get; set => SetField(ref field, value); }

    public int SelectedSkillIndex { get; set => SetField(ref field, value); } = -1;
    public Vector2I? SelectedTileCoords { get; set => SetField(ref field, value); }
    public bool WaitConfirm { get; set => SetField(ref field, value); } = true;
    public long SelectedUnitId { get; set => SetField(ref field, value); }
    public Vector2I CurrentMouseCoords { get; private set; }
    public IPlayerOperationSynchronizer PlayerOperationSynchronizer { get; private set; } = null!;
    
    public long FocusedUnitId { get; set => SetField(ref field, value); }
    public IGameView GameView { get; private set; } = null!;
    
    private GDTaskCompletionSource? _turnCompletionSource;
    private StateMachine<State> _stateMachine;
    private FreeState _freeState;
    private SkillState _skillState;
    private BusyState _busyState;
    private SelectState _selectState;
    
    private Vector2I _currentCoords;
    public event PropertyChangedEventHandler? PropertyChanged;
    protected StatePlayer()
    {
        _freeState = new(this);
        _skillState = new(this);
        _busyState = new(this);
        _selectState = new(this);
        
        _stateMachine = new StateMachine<State>(_freeState);
    }

    public static StatePlayer Create(long id, IGameView gameView, IEventScheduler gameScheduler, IPlayerOperationSynchronizer playerOperationSynchronizer)
    {
        var player = new StatePlayer();
        player.Id = id;
        player.GameView = gameView;
        player.GameScheduler = gameScheduler;
        player.PlayerOperationSynchronizer = playerOperationSynchronizer;
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
                if (e.Pressed && (e.ButtonMask.HasFlag(MouseButtonMask.Left) || e.ButtonMask.HasFlag(MouseButtonMask.Right)))
                    OnInput(new PlayerInputClick(_currentCoords, 
                        e.ButtonMask.HasFlag(MouseButtonMask.Left) ? 
                            PlayerClickTypeEnum.Left : PlayerClickTypeEnum.Right));
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
        if (!IsTurn || WaitConfirm) return;
        _stateMachine.CurrentState.OnInput(input);
    }

    public long Id { get; private set; }

    public void Init()
    {
        _stateMachine.Init();
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