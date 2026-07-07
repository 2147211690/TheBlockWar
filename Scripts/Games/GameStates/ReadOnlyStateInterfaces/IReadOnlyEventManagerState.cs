using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyEventManagerState : IReadOnlyGameDataState<IEventManagerState, EventManagerData>
{
    IReadOnlyDictionary<long, EventState> Events { get; }
    Vector2I Size { get; }
    IReadOnlyList<EventState> GetCoordsEvents(Vector2I coords);
    IReadOnlyList<EventState> GetUnitEvents(long unitId);
    IReadOnlyList<EventState> GetTeamEvents(int team);
    EventState? PeekFirstNotConsumed(ICollection<long> consumedIds);
    event EventHandler? EnabledEventUpdated;
}