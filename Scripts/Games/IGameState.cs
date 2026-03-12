using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games;

public interface IGameState : IReadOnlyGameState
{
    new uint FocusedUnitId { get; set; }
    new IMapState MapState { get; }
    void RemoveUnit(uint id);
    void AddUnit(in UnitState unit);
    void ModifyUnit(uint id, Func<UnitState, UnitState> modifier);
    
}