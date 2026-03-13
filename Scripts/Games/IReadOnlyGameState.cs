using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games;

public interface IReadOnlyGameState
{
    IReadOnlyTilesState TilesState { get; }
    IEnumerable<IReadOnlyUnitState> GetAllUnits();
    bool HasUnit(uint id);
    IReadOnlyUnitState? GetUnit(uint id);
    IReadOnlyUnitState? GetUnit(Vector2I coords);
    
    IGameState GetSnapshot();
}