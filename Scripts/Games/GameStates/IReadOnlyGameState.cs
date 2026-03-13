using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyGameState
{
    IReadOnlyTileMapState TileMapState { get; }
    IReadOnlyUnitMapState UnitMapState { get; }
    IGameState GetSnapshot();
}