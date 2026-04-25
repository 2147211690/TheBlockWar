using TheBlockWar.Scripts.Games.GameStates.Datas;
using Yitter.IdGenerator;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyGameState
{
    Vector2I Size { get; }
    DefaultIdGenerator IdGenerator { get; }
    DateTime BaseTime { get; }
    IReadOnlyGameState? Owner { get; }
    IReadOnlyTileMapState TileMapState { get; }
    IReadOnlyUnitMapState UnitMapState { get; }
    TeamState TeamState { get; }
    IReadOnlyList<ISkill> GetUnitValidSkills(string name);
    IGameState GetSnapshot();
    GameStateData ToData();
}