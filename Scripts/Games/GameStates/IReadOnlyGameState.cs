using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyGameState
{
    IReadOnlyGameState? Owner { get; }
    IReadOnlyTileMapState TileMapState { get; }
    IReadOnlyUnitMapState UnitMapState { get; }
    TeamState TeamState { get; }
    IReadOnlyList<ISkill> GetUnitValidSkills(string name);
    IGameState GetSnapshot();
}