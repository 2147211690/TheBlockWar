using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates.Datas;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IGameState : IReadOnlyGameState
{
    new IDictionary<long, SchedulerEvent>? Events { get; }
    new ITileMapState TileMapState { get; }
    new IUnitMapState UnitMapState { get; }
    void LoadGameData(GameStateData data);
    void ClearIfSnapshot();
}