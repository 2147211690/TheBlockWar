using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IGameState : IReadOnlyGameState
{
    new IDictionary<long, SchedulerEvent>? Events { get; }
    new ITileMapState TileMapState { get; }
    new IUnitMapState UnitMapState { get; }
    void ClearIfSnapshot();
}