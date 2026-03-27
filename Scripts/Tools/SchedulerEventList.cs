using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Tools;

public class SchedulerEventList
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public ICollection<SchedulerEvent> RunningEvents => _runningEvents;
    public ICollection<SchedulerEvent> WaitingEvents => _waitingEvents;
    public ICollection<SchedulerEvent> FocusedEvents => _focusedEvents;
    public IReadOnlyDictionary<long, SkillSchedulerEvent> SkillEvents => _skillEvents;
    private readonly OrderedSet<SchedulerEvent> _runningEvents = new();
    private readonly OrderedSet<SchedulerEvent> _waitingEvents = new();
    private readonly OrderedSet<SchedulerEvent> _focusedEvents = new();
    private readonly OrderedSet<SchedulerEvent>[] _eventsList;
    private readonly Dictionary<long, SkillSchedulerEvent> _skillEvents = new();
    private int _blockingCount = 0;
    private bool _needUpdate = false;
    public event EventHandler? ListUpdated;
    
    public SchedulerEventList()
    {
        _eventsList = [_runningEvents, _focusedEvents, _waitingEvents];
    }
    
    public void AddEvent(SchedulerEvent ev, SimulationLayer? layer = null)
    {
        BlockUpdate();
        if (layer != null) ev.Layer = layer.Value;
        if (!TryAddToList(ev))
        {
            UnblockUpdate();
            return;
        }
        OnListUpdate();
        UnblockUpdate();
    }

    private bool TryAddToList(SchedulerEvent ev)
    {
        var bag = GetSet(ev.Layer);
        if (bag.Contains(ev)) return false;
        if (ev is SkillSchedulerEvent skillEv)
        {
            if (!TryAddToSkill(skillEv)) return false;
        }
        bag.Add(ev);
        ev.PropertyChange += EvOnPropertyChange;
        return true;
    }

    public void AddEventMany(IEnumerable<SchedulerEvent> evs, SimulationLayer? layer = null)
    {
        BlockUpdate();
        foreach (var e in evs) AddEvent(e, layer);
        UnblockUpdate();
    }
    public void RemoveEventMany(IEnumerable<SchedulerEvent> evs)
    {
        BlockUpdate();
        foreach (var e in evs) RemoveEvent(e);
        UnblockUpdate();
    }
    private void EvOnPropertyChange(object? sender, (string? PropertyName, bool IsChanging) e)
    {
        var ev = (SchedulerEvent)sender!;
        if (e.PropertyName is nameof(SchedulerEvent.Layer) or nameof(SchedulerEvent.Turn) or nameof(SchedulerEvent.Priority))
        {
            var bag = GetSet(ev.Layer);
            if (e.IsChanging)
            {
                bag.Remove(ev);
            }
            else
            {
                bag.Add(ev);
            }
        }
        else if (ev is SkillSchedulerEvent skillEv && e.PropertyName == nameof(SkillSchedulerEvent.BindUnitId))
        {
            if (e.IsChanging)
            {
                _skillEvents.Remove(skillEv.BindUnitId);
            }
            else
            {
                TryAddToSkill(skillEv);
            }
        }
        if (!e.IsChanging) OnListUpdate();
    }
    private void OnListUpdate()
    {
        if (_blockingCount > 0)
        {
            _needUpdate = true;
            return;
        }
        ListUpdated?.Invoke(this, EventArgs.Empty);
        _needUpdate = false;
    }
    private bool TryAddToSkill(SkillSchedulerEvent skillEv)
    {
        if (!_skillEvents.TryAdd(skillEv.BindUnitId, skillEv))
        {
            Logger.Warn("单位{BindUnitId}已存在技能事件", skillEv.BindUnitId);
            return false;
        }
        return true;
    }

    private bool TryRemoveFromSkill(SkillSchedulerEvent skillEv)
    {
        return _skillEvents.Remove(skillEv.BindUnitId);
    }
    public void RemoveEvent(SchedulerEvent ev)
    {
        if (!TryRemoveFromList(ev)) return;
        if (ev is SkillSchedulerEvent skillEv) TryRemoveFromSkill(skillEv);
        OnListUpdate();
    }

    private bool TryRemoveFromList(SchedulerEvent ev)
    {
        var bag = GetSet(ev.Layer);
        if (!bag.Remove(ev)) return false;
        ev.PropertyChange -= EvOnPropertyChange;
        return true;
    }

    public void ClearLayerEvent(SimulationLayer layer)
    {
        var bag = GetSet(layer);
        foreach (var ev in bag)
        {
            if (ev is SkillSchedulerEvent skillEv) TryRemoveFromSkill(skillEv);
            ev.PropertyChange -= EvOnPropertyChange;
        }
        bag.Clear();
        OnListUpdate();
    }
    public void BlockUpdate()
    {
        _blockingCount++;
    }
    public void UnblockUpdate()
    {
        _blockingCount--;
        if (_blockingCount == 0 && _needUpdate) OnListUpdate();
    }
    private OrderedSet<SchedulerEvent> GetSet(SimulationLayer layer)
    {
        return _eventsList[(int)layer];
    }
}