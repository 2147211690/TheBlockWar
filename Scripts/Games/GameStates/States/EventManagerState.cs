using System.Linq;
using TheBlockWar.Scripts.Games.GameStates.Datas;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.GameStates;

public class EventManagerState : IEventManagerState
{
    public IReadOnlyDictionary<long, EventState> Events => _events;
    public Vector2I Size { get; set; } = Vector2I.Zero;
    private Dictionary<long, EventState> _events { get; }
    private MultiDictionary<Vector2I, EventState> _coordsEvents = new(false);
    private MultiDictionary<long, EventState> _unitEvents = new(false);
    
    private Dictionary<Vector2I, IReadOnlyList<EventState>?> _coordsEventsCache = new();
    private Dictionary<long, IReadOnlyList<EventState>?> _unitEventsCache = new();
    private Dictionary<int, IReadOnlyList<EventState>?> _teamEventsCache = new();
    public event EventHandler? EnabledEventUpdated;

    public EventManagerState(Vector2I size)
    {
        _events = new();
        Size = size;
    }
    public EventManagerState(EventManagerData data)
    {
        _events = new(data.Events);
        Size = data.Size;
    }
    
    public EventState AddEvent(EventState eventState)
    {
        _events.Add(eventState.Id, eventState);
        if (eventState.Coords != null)
        {
            _coordsEvents.Add(eventState.Coords.Value, eventState);
            _coordsEventsCache.Remove(eventState.Coords.Value);
        }
        var bindId = eventState.BindUnitId;
        if (bindId != 0)
        {
            _unitEvents.Add(bindId, eventState);
            _unitEventsCache.Remove(bindId);
        }
       
        _teamEventsCache.Remove(eventState.Team);
        eventState.PropertyChanged += OnEventPropertyChanged;
        
        if (eventState.Layer != SimulationLayer.Disabled) EnabledEventUpdated?.Invoke(this, EventArgs.Empty);
        return eventState;
    }
    public void RemoveEvent(long id)
    {
        if (!_events.Remove(id, out var eventState)) return;
        if (eventState.Coords != null)
        {
            _coordsEvents.Remove(eventState.Coords.Value, eventState);
            _coordsEventsCache.Remove(eventState.Coords.Value);
        }
        var bindId = eventState.BindUnitId;
        if (bindId != 0)
        {
            _unitEvents.Remove(bindId, eventState);
            _unitEventsCache.Remove(bindId);
        }
        _teamEventsCache.Remove(eventState.Team);
        eventState.PropertyChanged -= OnEventPropertyChanged;
        if (eventState.Layer != SimulationLayer.Disabled) EnabledEventUpdated?.Invoke(this, EventArgs.Empty);
    }
    private void OnEventPropertyChanged(object? sender, (string PropertyName, bool IsChanging) e)
    {
        var eventState = (EventState)sender!;
        if (e.IsChanging)
        {
            if (e.PropertyName is nameof(EventState.Core))
            {
                var bindId = eventState.BindUnitId;
                if (bindId != 0)
                {
                    _unitEvents.Remove(bindId, eventState);
                    _unitEventsCache.Remove(bindId);
                }
                var coords = eventState.Coords;
                if (coords != null)
                {
                    _coordsEvents.Remove(coords.Value, eventState);
                    _coordsEventsCache.Remove(coords.Value);
                }
            }
            else if (e.PropertyName is nameof(EventState.Team))
            {
                _teamEventsCache.Remove(eventState.Team);
            }
        }
        else
        {
            if (e.PropertyName is nameof(EventState.Core))
            {
                var bindId = eventState.BindUnitId;
                if (bindId != 0)
                {
                    _unitEvents.Add(bindId, eventState);
                    _unitEventsCache.Remove(bindId);
                }
                var coords = eventState.Coords;
                if (coords != null && coords.Value.IsInSize(Size))
                {
                    _coordsEvents.Add(coords.Value, eventState);
                    _coordsEventsCache.Remove(coords.Value);
                }
            }
            else if (e.PropertyName is nameof(EventState.Team))
            {
                _teamEventsCache.Remove(eventState.Team);
            }

        }
        if (!e.IsChanging && (e.PropertyName is nameof(EventState.Layer) || eventState.Layer != SimulationLayer.Disabled)) 
            EnabledEventUpdated?.Invoke(this, EventArgs.Empty);
    }
    public IReadOnlyList<EventState> GetCoordsEvents(Vector2I coords)
    {
        if (!coords.IsInSize(Size)) return [];
        if (!_coordsEventsCache.TryGetValue(coords, out var events))
        {
            var list = new List<EventState>();
            foreach (var eventState in _coordsEvents[coords])
            {
                list.Add(eventState);
            }
            _coordsEventsCache.Add(coords, list.Count > 0 ? list : null);
            events = list;
        }
        return events ?? [];
    }

    public IReadOnlyList<EventState> GetUnitEvents(long unitId)
    {
        if (!_unitEventsCache.TryGetValue(unitId, out var events))
        {
            var list = new List<EventState>();
            foreach (var eventState in _unitEvents[unitId])
            {
                list.Add(eventState);
            }
            _unitEventsCache.Add(unitId, list.Count > 0 ? list : null);
            events = list;
        }
        return events ?? [];
    }

    public IReadOnlyList<EventState> GetTeamEvents(int team)
    {
        if (!_teamEventsCache.TryGetValue(team, out var events))
        {
            var list = new List<EventState>();
            foreach (var eventState in _events.Values)
            {
                if (eventState.Team == team)
                {
                    list.Add(eventState);
                }
            }
            _teamEventsCache.Add(team, list.Count > 0 ? list : null);
            events = list;
        }
        return events ?? [];
    }
    
    
    public EventState? PeekFirstNotConsumed(ICollection<long> consumedIds)
    {
        EventState? result = null;
        foreach (var eventState in _events.Values)
        {
            if (consumedIds.Contains(eventState.Id))
                continue;
            if (result is null || EventState.SimulationComparer.Compare(eventState, result) < 0)
                result = eventState;
        }
        return result;
    }


    public EventManagerData ToData()
    {
        return new EventManagerData()
        {
            Events = _events,
            Size = Size,
        };
    }

    public IEventManagerState GetSnapshot()
    {
        return new EventManagerStateSnapshot(this);
    }
    
    public void ClearIfSnapshot()
    {
    }
    
    private class EventManagerStateSnapshot : IEventManagerState
    {
        public event EventHandler? EnabledEventUpdated { add {} remove {} }

        private IReadOnlyEventManagerState Owner { get; }
        private readonly Dictionary<long, EventState> _events;
        private readonly MultiDictionary<Vector2I, EventState> _coordsEvents;
        private readonly MultiDictionary<long, EventState> _unitEvents;
        private readonly List<EventState> _pooledEvents;

        private static readonly TypedPool<EventState> EventStatePool = new(
            () => new EventState(0), 10, afterReturn: e => e.Clear());

        public EventManagerStateSnapshot(IReadOnlyEventManagerState owner)
        {
            Owner = owner;
            _events = new Dictionary<long, EventState>(owner.Events.Count);
            _coordsEvents = new MultiDictionary<Vector2I, EventState>(false);
            _unitEvents = new MultiDictionary<long, EventState>(false);
            _pooledEvents = new List<EventState>(owner.Events.Count);
            CloneFromOwner();
        }

        private void CloneFromOwner()
        {
            foreach (var (id, eventState) in Owner.Events)
            {
                var clone = EventStatePool.Get();
                eventState.CloneTo(clone);
                _events.Add(id, clone);
                _pooledEvents.Add(clone);

                if (clone.Coords != null)
                    _coordsEvents.Add(clone.Coords.Value, clone);
                var bindId = clone.BindUnitId;
                if (bindId != 0)
                    _unitEvents.Add(bindId, clone);

                clone.PropertyChanged += OnEventPropertyChanged;
            }
        }

        private void OnEventPropertyChanged(object? sender, (string PropertyName, bool IsChanging) e)
        {
            var eventState = (EventState)sender!;
            if (e.IsChanging)
            {
                if (e.PropertyName is nameof(EventState.Core))
                {
                    var bindId = eventState.BindUnitId;
                    if (bindId != 0)
                        _unitEvents.Remove(bindId, eventState);
                    var coords = eventState.Coords;
                    if (coords != null)
                        _coordsEvents.Remove(coords.Value, eventState);
                }
            }
            else
            {
                if (e.PropertyName is nameof(EventState.Coords))
                {
                    if (eventState.Coords != null)
                        _coordsEvents.Add(eventState.Coords.Value, eventState);
                }
                else if (e.PropertyName is nameof(EventState.Core))
                {
                    var bindId = eventState.BindUnitId;
                    if (bindId != 0)
                        _unitEvents.Add(bindId, eventState);
                    var coords = eventState.Coords;
                    if (coords != null && coords.Value.IsInSize(Size))
                        _coordsEvents.Add(coords.Value, eventState);
                }
            }
        }

        public IReadOnlyDictionary<long, EventState> Events => _events;
        public Vector2I Size => Owner.Size;

        public EventManagerData ToData()
        {
            return new EventManagerData { Events = _events, Size = Owner.Size };
        }

        public IEventManagerState GetSnapshot()
        {
            return new EventManagerStateSnapshot(this);
        }

        public IReadOnlyList<EventState> GetCoordsEvents(Vector2I coords)
        {
            return _coordsEvents[coords].ToList();
        }

        public IReadOnlyList<EventState> GetUnitEvents(long unitId)
        {
            return _unitEvents[unitId].ToList();
        }

        public IReadOnlyList<EventState> GetTeamEvents(int team)
        {
            var list = new List<EventState>();
            foreach (var eventState in _events.Values)
            {
                if (eventState.Team == team)
                    list.Add(eventState);
            }
            return list;
        }
        

        public EventState? PeekFirstNotConsumed(ICollection<long> consumedIds)
        {
            EventState? result = null;
            foreach (var eventState in _events.Values)
            {
                if (consumedIds.Contains(eventState.Id))
                    continue;
                if (result is null || EventState.SimulationComparer.Compare(eventState, result) < 0)
                    result = eventState;
            }
            return result;
        }

        public void ClearIfSnapshot()
        {
            foreach (var e in _pooledEvents)
            {
                e.Clear();
                EventStatePool.Return(e);
            }
            _pooledEvents.Clear();
            _events.Clear();
            _coordsEvents.Clear();
            _unitEvents.Clear();
            CloneFromOwner();
        }

        public EventState AddEvent(EventState eventState)
        {
            _events.Add(eventState.Id, eventState);
            if (eventState.Coords != null)
                _coordsEvents.Add(eventState.Coords.Value, eventState);
            var bindId = eventState.BindUnitId;
            if (bindId != 0)
                _unitEvents.Add(bindId, eventState);
            eventState.PropertyChanged += OnEventPropertyChanged;
            return eventState;
        }

        public void RemoveEvent(long id)
        {
            if (!_events.Remove(id, out var eventState)) return;
            eventState.PropertyChanged -= OnEventPropertyChanged;
            if (eventState.Coords != null)
                _coordsEvents.Remove(eventState.Coords.Value, eventState);
            var bindId = eventState.BindUnitId;
            if (bindId != 0)
                _unitEvents.Remove(bindId, eventState);
        }
    }
}