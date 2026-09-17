using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates.Datas;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IGameState : IReadOnlyGameState, IGameDataState
{
    new ITileMapState TileMapState { get; }
    new IUnitMapState UnitMapState { get; }
    new IBuffGlobalState BuffGlobalState { get; }
    new ITurnState TurnState { get; }
    void LoadGameData(GameStateData data);
}