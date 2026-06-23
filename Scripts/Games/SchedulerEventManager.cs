using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games;

public class SchedulerEventManager
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
    private IDictionary<long, SchedulerEvent> StateEvents => _getStateEvents();

    private readonly Dictionary<long, SchedulerEvent> _skillEvents = new();
    private readonly Dictionary<int, List<SchedulerEvent>> _turnEvents = new();
    private readonly ICollection<SchedulerEvent>[] _eventsList;
    private int _blockingCount = 0;
    private bool _needUpdate = false;
    private readonly Func<IDictionary<long, SchedulerEvent>> _getStateEvents;
    private readonly IEventScheduler _eventScheduler;
    public event EventHandler? EnabledListUpdated;

    public SchedulerEventManager(IEventScheduler eventScheduler, Func<IDictionary<long, SchedulerEvent>> getStateEvents)
    {
        _eventScheduler = eventScheduler;
        _eventsList = [_runningEvents, _focusedEvents, _waitingEvents, _disabledEvents];
        _getStateEvents = getStateEvents;
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

    public void ReloadStateEvents()
    {
        _runningEvents.Clear();
        _waitingEvents.Clear();
        _focusedEvents.Clear();
        _disabledEvents.Clear();
        _turnEvents.Clear();
        _skillEvents.Clear();

        foreach (var ev in StateEvents.Values)
        {
            AddToList(ev);
            ev.PropertyChange += EvOnPropertyChange;
        }

        OnEnableListUpdate();
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
                        nameof(SkillEvent.SkillBindUnitId) or
                        nameof(SchedulerEvent.PhaseOrder))
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
                    nameof(SchedulerEvent.Priority) or
                    nameof(SchedulerEvent.PhaseOrder))
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