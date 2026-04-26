/*using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Events;

namespace TheBlockWar.Scripts.Tools;

public class TurnEventManager
{
    private Func<SchedulerEvent, bool> _onTurnEventEnter;
    private MultiDictionary<int, SchedulerEvent> _turnNormalEvents = new(false);
    private MultiDictionary<int, SchedulerEvent> _turnSkillEvents = new(false);
    
    public TurnEventManager(Func<SchedulerEvent, bool> onTurnEventEnter)
    {
        _onTurnEventEnter = onTurnEventEnter;
    }

    public ICollection<SchedulerEvent> GetTurnNormalEvents(int turn)
    {
        return _turnNormalEvents[turn];
    }
    public ICollection<SchedulerEvent> GetTurnSkillEvents(int turn)
    {
        return _turnSkillEvents[turn];
    }

    public void AddTurnEvent(SchedulerEvent ev)
    {
        if (!_onTurnEventEnter.Invoke(ev)) return;
        if (ev.Type == SchedulerEventType.Skill)
        {
            _turnSkillEvents.Add(ev.Turn, ev);
        }
        else
        {
            _turnNormalEvents.Add(ev.Turn, ev);
        }
        ev.PropertyChange += EvOnPropertyChange;
    }

    private void EvOnPropertyChange(object? sender, (string? PropertyName, bool IsChanging) e)
    {
        var ev = (SchedulerEvent)sender!;
        if (e.PropertyName == nameof(SchedulerEvent.Turn))
        {
            if (e.IsChanging)
            {
                if (ev.Type == SchedulerEventType.Skill)
                {
                    _turnSkillEvents.Remove(ev.Turn, ev);
                }
                else
                {
                    _turnNormalEvents.Remove(ev.Turn, ev);
                }
            }
            else
            {
                if (!_onTurnEventEnter.Invoke((SchedulerEvent)sender!))
                {
                    ev.PropertyChange -= EvOnPropertyChange;
                    return;
                }
                if (ev.Type == SchedulerEventType.Skill)
                {
                    _turnSkillEvents.Add(ev.Turn, ev);
                }
                else
                {
                    _turnNormalEvents.Add(ev.Turn, ev);
                }
            }
        }
    }
    
    public void RemoveTurnSkillEvents(int turn)
    {
        foreach (var skillEv in _turnSkillEvents[turn])
        {
            skillEv.PropertyChange -= EvOnPropertyChange;
        }
        _turnSkillEvents.Remove(turn);
    }
    public void RemoveTurnNormalEvents(int turn)
    {
        foreach (var normalEv in _turnNormalEvents[turn])
        {
            normalEv.PropertyChange -= EvOnPropertyChange;
        }
        _turnNormalEvents.Remove(turn); 
    }
}*/