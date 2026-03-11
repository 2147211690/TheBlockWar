using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games;

public interface IGameState
{
    IMapState MapState { get; }
    IEnumerable<UnitState> GetAllUnits();
    bool HasUnit(uint id);
    UnitState? GetUnit(uint id);
    UnitState? GetUnit(Vector2I coords);
    void RemoveUnit(uint id);
    void AddUnit(in UnitState unit);
    void ModifyUnit(uint id, Func<UnitState, UnitState> modifier);
    
    IGameState GetSnapshot();
}