using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates.Datas;
using TheBlockWar.Scripts.Games.Skills;
using Yitter.IdGenerator;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyGameState : IReadOnlyGameDataState<IGameState, GameStateData>
{
    IReadOnlyDictionary<long, SchedulerEvent>? Events { get; }
    bool IsSnapshot { get; }
    Vector2I Size { get; }
    DefaultIdGenerator IdGenerator { get; }
    DateTime BaseTime { get; }
    IReadOnlyGameState? Owner { get; }
    IReadOnlyTileMapState TileMapState { get; }
    IReadOnlyUnitMapState UnitMapState { get; }
    IReadOnlyBuffGlobalState BuffGlobalState { get; }
    IReadOnlyTurnState TurnState { get; }
    TeamState TeamState { get; }
    IReadOnlyList<ISkill> GetUnitValidSkills(string unitName);
}