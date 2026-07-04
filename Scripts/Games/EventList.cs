using TheBlockWar.Scripts.Games.Events;

namespace TheBlockWar.Scripts.Games;

public class EventList
{
    private readonly List<SchedulerEvent> _events = new();

    public IEnumerable<SchedulerEvent> HandleEvents()
    {
        while (_events.Count > 0)
        {
            _events.Sort(SchedulerEvent.DefaultCompare);
            var r = _events[0];
            _events.RemoveAt(0);
            yield return r;
        }
    }
    
    public void AddEvent(SchedulerEvent e)
    {

    }

    public void RemoveEvent(SchedulerEvent e)
    {
        
    }
}