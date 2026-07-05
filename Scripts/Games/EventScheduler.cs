using System.Linq;
using System.Text;
using System.Text.Json;
using NLog;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Tools.Jsons;

namespace TheBlockWar.Scripts.Games;

/// <summary>
/// 控制游戏的总调度器,请在任何与游戏状态相关的操作中使用.
/// </summary>
public class EventScheduler : IEventScheduler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static Stack<IEventScheduler> _runningSchedulers = new();

    /// <summary>
    /// 当前正在运行的调度器,如果没有正在运行的调度器,则为null
    /// 注意不要在模拟时更新调度器内容,仅用来查询当前游戏状态
    /// </summary>
    public static IEventScheduler CurrentScheduler => _runningSchedulers.Peek();
    
    public IReadOnlyGameState GameState => _gameState;
    public int PlayerTeam { get; }
    public int CurrentTurn { get => _gameState.TurnState.CurrentTurn; set => _gameState.TurnState.CurrentTurn = value; }
    private IGameState _gameState;
    private IGameState? _snapshotChache;
    public event EventHandler<SchedulerResult>? SimulationUpdated;
    public event EventHandler<IReadOnlyGameState>? GameStateReload;

    public bool IsRunning
    {
        get;
        set
        { 
            if (field == value) return;
            field = value;
            if (field)
            {
                _runningCompletionSource = new();
                _runningCompletionSource.Task.Forget();
            }
            else
            {
                _runningCompletionSource!.TrySetResult();
                _runningCompletionSource = null;
            }
        }
    }
    private GDTaskCompletionSource? _runningCompletionSource;

    private bool _isSimulating = false;
    private IEventScheduler _eventSchedulerImplementation;
    private int _updateCount = 0;
    private int _blockCount = 0;
    public EventScheduler(IGameState gameState, int playerTeam)
    {
        _gameState = gameState;
        PlayerTeam = playerTeam;
        _gameState.EventManagerState.EnabledEventUpdated += OnEnabledEventUpdated;
    }

    private void OnEnabledEventUpdated(object? sender, EventArgs e)
    {
        UpdateSimulation();
    }

    public void ReloadGameState()
    {
        GameStateReload?.Invoke(this, GameState);
        UpdateSimulation();
    }

    public void BlockUpdate()
    {
        _blockCount++;
    }

    public void UnblockUpdate()
    {
        _blockCount--;
        if (_blockCount <= 0 && _updateCount > 0)
        {
            UpdateSimulation();
        }
    }

    public GDTask WaitCompleteRunning() => _runningCompletionSource?.Task ?? GDTask.CompletedTask;
    public IReadOnlyDictionary<long, EventState> Events => _gameState.EventManagerState.Events;
    public IReadOnlyList<EventState> GetCoordsEvents(Vector2I coords)
        => _gameState.EventManagerState.GetCoordsEvents(coords);
    public IReadOnlyList<EventState> GetUnitEvents(long unitId)
        => _gameState.EventManagerState.GetUnitEvents(unitId);
    public IReadOnlyList<EventState> GetTeamEvents(int team)
        => _gameState.EventManagerState.GetTeamEvents(team);
    public IReadOnlyList<EventState> GetTurnEvents(int turn)
        => _gameState.EventManagerState.GetTurnEvents(turn);
    public EventState CreateEvent(EventCore core,
        int turn, int phaseOrder, int team,
        SimulationLayer layer,
        int priority = 0)
        => _gameState.CreateEvent(core, turn, phaseOrder, team, layer, priority);
    public void RemoveEvent(long id)
        => _gameState.EventManagerState.RemoveEvent(id);

    public void UpdateSimulation()
    {
        if (_isSimulating) return;
        if (_blockCount > 0)
        {
            _updateCount++;
            return;
        }
        
        _isSimulating = true;
        _runningSchedulers.Push(this);
        
        Logger.Info("触发更新");
        var result = new SchedulerResult();
        
        // 创建快照，用于焦点层和等待层事件执行
        var snapshot = _snapshotChache =
            (_snapshotChache is null || _snapshotChache.Owner != GameState)
                ? _gameState.GetSnapshot()
                : _snapshotChache;
        snapshot.ClearIfSnapshot();
        // 开始更新
        var consumed = new HashSet<long>();
        var currentEvent = snapshot.EventManagerState.PeekFirstNotConsumed(consumed);
        while (currentEvent != null)
        {
            if (currentEvent.Layer == SimulationLayer.Disabled)
            {
                break;
            }
            
            if (!currentEvent.CheckIsValid(snapshot))
            {
                consumed.Add(currentEvent.Id);
                continue;
            }
            
            //执行一个事件
            var obj = new SimObject(snapshot, currentEvent.Layer, new(), currentEvent.Id);
            obj.PushContext(new SimContext(currentEvent.BindUnitId));
            currentEvent.Apply(obj);
            //如果是运行层,删除
            if (currentEvent.Layer == SimulationLayer.Running) obj.ApplyRemoveEvent(currentEvent.Id);
            obj.PopContext();
            consumed.Add(currentEvent.Id); //标记为已执行
                       
            //记录结果
            result.Sims.Add(obj);
            result.RunningCount += currentEvent.Layer == SimulationLayer.Running ? 1 : 0;
            
            //获取下一个事件
            currentEvent = snapshot.EventManagerState.PeekFirstNotConsumed(consumed);
        }

        //结果应用到游戏状态
        for (var i = 0; i < result.RunningCount; i++)
        {
            result.Sims[i].Steps.Replay(_gameState);
        }
        
        //做一些清理
        _runningSchedulers.Pop();
        _isSimulating = false;
        _blockCount = 0;
        _updateCount = 0;
        Logger.Trace(result.Sims.ToTree());
        SimulationUpdated?.Invoke(this, result);
    }
}