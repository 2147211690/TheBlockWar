using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyUnitMapState
{
    IEnumerable<IReadOnlyUnitState> GetAllUnits();
    bool HasUnit(uint id);
    bool HasUnit(Vector2I coords);
    IReadOnlyUnitState? GetUnit(uint id);
    IReadOnlyUnitState? GetUnit(Vector2I coords);
    
    IUnitMapState GetSnapshot();
}