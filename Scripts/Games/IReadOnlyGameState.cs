using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games;

public interface IReadOnlyGameState
{
    IReadOnlyMapState MapState { get; }
    IEnumerable<UnitState> GetAllUnits();
    uint FocusedUnitId { get; }
    bool HasUnit(uint id);
    UnitState? GetUnit(uint id);
    UnitState? GetUnit(Vector2I coords);
    
    IGameState GetSnapshot();
}