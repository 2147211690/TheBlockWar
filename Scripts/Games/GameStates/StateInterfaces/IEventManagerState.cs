using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IEventManagerState : IReadOnlyEventManagerState, IGameDataState
{
    EventState AddEvent(EventState eventState);
    void RemoveEvent(long id);
}