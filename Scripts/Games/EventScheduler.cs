using System.Linq;
using System.Text;
using NLog;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games;

/// <summary>
/// 控制游戏的总调度器,请在任何与游戏状态相关的操作中使用.
/// </summary>
public class EventScheduler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static Stack<EventScheduler> _runningSchedulers = new();

    /// <summary>
    /// 当前正在运行的调度器,如果没有正在运行的调度器,则为null
    /// 注意不要在模拟时更新调度器内容,仅用来查询当前游戏状态
    /// </summary>
    public static EventScheduler CurrentScheduler => _runningSchedulers.Peek();

    public ICollection<SchedulerEvent> RunningEvents => _eventManager.RunningEvents;
    public ICollection<SchedulerEvent> FocusedEvents => _eventManager.FocusedEvents;
    public ICollection<SchedulerEvent> WaitingEvents => _eventManager.WaitingEvents;
    public ICollection<SchedulerEvent> DisabledEvents => _eventManager.DisabledEvents;
    public IReadOnlyDictionary<long, SchedulerEvent> SkillEvents => _eventManager.SkillEvents;
    public IReadOnlyGameState GameState => _gameState;
    public long FocusedUnitId { get; private set; }
    public int PlayerTeam { get; }
    public IReadOnlyDictionary<long, (HashSet<Vector2I> Coords, int type)> CoordsRegions => _coordsRegions;

    private SchedulerEventManager _eventManager;

    private IGameState _gameState;
    private Dictionary<long, (HashSet<Vector2I> Coords, int type)> _coordsRegions = new();
    private long _coordsRegionsId;
    private IGameState? _snapshotChache;

    public event EventHandler<List<SimulationContext>>? SimulationUpdated;
    public event EventHandler<OtherStateUpdateTypeEnum>? OtherStateUpdated;
    public event EventHandler<int>? NextDelay;

    private bool _isRunning = false;

    public EventScheduler(IGameState gameState, int playerTeam)
    {
        _gameState = gameState;
        PlayerTeam = playerTeam;
        _eventManager = new(this);
        _eventManager.EnabledListUpdated += OnEventEnabledManagerUpdate;
    }

    private void OnEventEnabledManagerUpdate(object? sender, EventArgs e)
    {
        UpdateSimulation();
    }

    public void FocusUnit(long id)
    {
        if (FocusedUnitId == id) return;
        FocusedUnitId = id;
        OtherStateUpdated?.Invoke(this, OtherStateUpdateTypeEnum.FocusedUnitId);
    }

    public void SetNextCompleteDelay(int delay)
    {
        NextDelay?.Invoke(this, delay);
    }

    public SchedulerEvent? GetUnitSkillEvent(long unitId)
    {
        SkillEvents.TryGetValue(unitId, out var skillEvent);
        return skillEvent;
    }
    
    public T CreateEvent<T>(Func<long, T> createFunc) where T : SchedulerEvent
    {
        return _eventManager.CreateEvent(createFunc);
    }
    public void DeleteEvent(SchedulerEvent ev)
    {
        _eventManager.DeleteEvent(ev);
    }
    public void DeleteEvent(long id)
    {
        _eventManager.DeleteEvent(id);
    }

    public void BlockUpdate()
    {
        _eventManager.BlockUpdate();
    }

    public void UnblockUpdate()
    {
        _eventManager.UnblockUpdate();
    }
    public IReadOnlyList<SchedulerEvent> GetTurnEvents(int turn) => _eventManager.GetTurnEvents(turn);

    public long AddCoordsRegion(IEnumerable<Vector2I> coords, int type)
    {
        var id = _coordsRegionsId++;
        _coordsRegions[id] = (coords.ToHashSet(), type);
        OtherStateUpdated?.Invoke(this, OtherStateUpdateTypeEnum.CoordsRegion);
        return id;
    }

    public bool IsInCoordsRegion(long id, in Vector2I coord) =>
        CoordsRegions.TryGetValue(id, out var region) && region.Coords.Contains(coord);

    public void RemoveCoordsRegion(long id)
    {
        _coordsRegions.Remove(id);
        OtherStateUpdated?.Invoke(this, OtherStateUpdateTypeEnum.CoordsRegion);
    }

    private void UpdateSimulation()
    {
        if (_isRunning) return;
        _isRunning = true;
        Logger.Info("触发更新");
        var contexts = new List<SimulationContext>();
        _runningSchedulers.Push(this);

        // 运行层直接执行
        foreach (var ev in _eventManager.RunningEvents)
        {
            var context = CreateSimulationContext(ev);
            ev.OnApply(ev, _gameState, context);
            contexts.Add(context);
        }
        // 创建快照，用于焦点层和等待层事件执行
        var snapshot = _snapshotChache =
            (_snapshotChache is null || _snapshotChache.Owner != GameState)
                ? _gameState.GetSnapshot()
                : _snapshotChache;
        snapshot.ClearIfSnapshot();
        // 焦点层和等待层事件执行
        foreach (var ev in _eventManager.FocusedEvents.Concat(_eventManager.WaitingEvents))
        {
            var context = CreateSimulationContext(ev);
            ev.OnApply(ev, snapshot, context);
            contexts.Add(context);
        }
        
        var sb = new StringBuilder();
        foreach (var ev in contexts)
        {
            sb.AppendLine("==========");
            if (_gameState.Events!.TryGetValue(ev.SourceId, out var value)) sb.AppendLine(value.ToString());
            sb.AppendLine("---");
            sb.AppendLine(ev.ToString());
            sb.AppendLine("==========");
        }
        Logger.Debug("更新结果:\n{results}", sb.ToString());
        Logger.Debug("结构树:\n{tree}", contexts.SelectMany(a => a.Steps ?? []).ToTree().ToString());
        
        // 运行层事件执行完毕后，清空运行层
        _eventManager.ClearLayerEvent(SimulationLayer.Running);
        
        _runningSchedulers.Pop();
        _isRunning = false;
        SimulationUpdated?.Invoke(this, contexts);
    }

    private SimulationContext CreateSimulationContext(SchedulerEvent ev)
    {
        var context = new SimulationContext()
        {
            SourceId = ev.Id,
            Layer = ev.Layer,
            TeamType = GameState.TeamState.GetTeamTypeRelativeToPlayer(PlayerTeam, ev.Team),
            SourceUnitId = ev is SkillEvent sev ? sev.SkillBindUnitId : 0,
        };
        return context;
    }

    public enum OtherStateUpdateTypeEnum
    {
        None,
        FocusedUnitId,
        CoordsRegion,
    }



    private class SchedulerEventManager
    {
        public ICollection<SchedulerEvent> RunningEvents => _runningEvents;
        public ICollection<SchedulerEvent> WaitingEvents => _waitingEvents;
        public ICollection<SchedulerEvent> FocusedEvents => _focusedEvents;
        public ICollection<SchedulerEvent> DisabledEvents => _disabledEvents;
        public IReadOnlyDictionary<long, SchedulerEvent> SkillEvents => _skillEvents;
        public IReadOnlyDictionary<long, SchedulerEvent> Events => _eventScheduler.GameState.Events!;

        private readonly OrderedSet<SchedulerEvent> _runningEvents = new();
        private readonly OrderedSet<SchedulerEvent> _waitingEvents = new();
        private readonly OrderedSet<SchedulerEvent> _focusedEvents = new();
        private readonly HashSet<SchedulerEvent> _disabledEvents = new();
        private IDictionary<long, SchedulerEvent> StateEvents => _eventScheduler._gameState.Events!;

        private readonly Dictionary<long, SchedulerEvent> _skillEvents = new();
        private readonly Dictionary<int, List<SchedulerEvent>> _turnEvents = new();
        private readonly ICollection<SchedulerEvent>[] _eventsList;
        private int _blockingCount = 0;
        private bool _needUpdate = false;
        private EventScheduler _eventScheduler;
        public event EventHandler? EnabledListUpdated;

        public SchedulerEventManager(EventScheduler eventScheduler)
        {
            _eventScheduler = eventScheduler;
            _eventsList = [_runningEvents, _focusedEvents, _waitingEvents, _disabledEvents];
            foreach (var ev in StateEvents.Values) AddToList(ev);
        }

        public IReadOnlyList<SchedulerEvent> GetTurnEvents(int turn)
        {
            if (_turnEvents.TryGetValue(turn, out var events)) return events;
            var r = _turnEvents[turn] = new();
            foreach (var ev in StateEvents.Values)
            {
                if (ev.Turn == turn) r.Add(ev);
            }
            r.Sort(SchedulerEvent.IgnoreLayerCompare);
            return r;
        }

        public void ClearTurnEvents(int turn = -1)
        {
            if (turn == -1) _turnEvents.Clear();
            else _turnEvents.Remove(turn);
        }
        
        public T CreateEvent<T>(Func<long, T> createEvent) where T : SchedulerEvent
        {
            var ev = createEvent(_eventScheduler.GameState.IdGenerator.NewLong());
            
            AddToList(ev);
            ev.PropertyChange += EvOnPropertyChange;
            StateEvents.Add(ev.Id, ev);
            ClearTurnEvents(ev.Turn);
            if (ev.Layer != SimulationLayer.Disabled) OnEnableListUpdate();
            
            return ev;
        }

        public void ClearLayerEvent(SimulationLayer layer)
        {
            var set = GetSet(layer);
            foreach (var ev in set)
            {
                if (ev is SkillEvent sev)
                {
                    _skillEvents.Remove(sev.SkillBindUnitId);
                }
                ev.PropertyChange -= EvOnPropertyChange;
                StateEvents.Remove(ev.Id);
                ClearTurnEvents(ev.Turn);
            }
            set.Clear();
            OnEnableListUpdate();
        }

        public void DeleteEvent(SchedulerEvent ev)
        {
            RemoveFromList(ev);
            ev.PropertyChange -= EvOnPropertyChange;
            StateEvents.Remove(ev.Id);
            ClearTurnEvents(ev.Turn);
            if (ev.Layer != SimulationLayer.Disabled) OnEnableListUpdate();
        }

        public void DeleteEvent(long id)
        {
            if (!StateEvents.TryGetValue(id, out var ev)) return;
            DeleteEvent(ev);
        }

        private void AddToList(SchedulerEvent ev)
        {
            var bag = GetSet(ev.Layer);
            bag.Add(ev);
            if (ev is SkillEvent sev) _skillEvents.Add(sev.SkillBindUnitId, ev);
        }
        
        private void RemoveFromList(SchedulerEvent ev)
        {
            var bag = GetSet(ev.Layer);
            bag.Remove(ev);
            if (ev is SkillEvent sev) _skillEvents.Remove(sev.SkillBindUnitId);
        }

        private void EvOnPropertyChange(object? sender, (string? PropertyName, bool IsChanging) e)
        {
            var ev = (SchedulerEvent)sender!;
            switch (e.PropertyName)
            {
                case nameof(SchedulerEvent.Layer): //层级引发改变,需要调整在列表的位置
                    if (e.IsChanging)
                    {
                        RemoveFromList(ev); //改变前先移除
                    }
                    else
                    {
                        AddToList(ev); //改变后添加
                        OnEnableListUpdate(); //通知列表更新
                    }
                    break;
                default:
                    if (ev.Layer != SimulationLayer.Disabled) //如果不是层改变 且 处于激活状态的属性改变
                    {
                        //如果引发在顺序相关的属性改变,则需要调整在列表的位置
                        if (e.PropertyName is 
                            nameof(SchedulerEvent.Priority) or 
                            nameof(SchedulerEvent.Turn) or
                            nameof(SkillEvent.SkillBindUnitId))
                        {
                            if (e.IsChanging)
                            {
                                RemoveFromList(ev);
                            }
                            else
                            {
                                AddToList(ev);
                            }
                        }

                        if (!e.IsChanging) OnEnableListUpdate();
                    }
                    //如果引发在Turn相关的属性改变,则需要清除当前回合的事件
                    if (e.PropertyName is 
                        nameof(SchedulerEvent.Turn) or 
                        nameof(SchedulerEvent.Priority))
                    {
                        if (e.IsChanging)
                        {
                            ClearTurnEvents(ev.Turn);
                        }
                        else
                        {
                            ClearTurnEvents(ev.Turn);
                        }
                    }
                    break;
            }
        }

        private void OnEnableListUpdate()
        {
            if (_blockingCount > 0)
            {
                _needUpdate = true;
                return;
            }

            EnabledListUpdated?.Invoke(this, EventArgs.Empty);
            _needUpdate = false;
        }

        public void BlockUpdate()
        {
            _blockingCount++;
        }

        public void UnblockUpdate()
        {
            _blockingCount--;
            if (_blockingCount == 0 && _needUpdate) OnEnableListUpdate();
        }

        private ICollection<SchedulerEvent> GetSet(SimulationLayer layer)
        {
            return _eventsList[(int)layer];
        }
    }
}