using System;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IUnitMapState : IReadOnlyUnitMapState
{
    void AddUnit(UnitState unit);
    void RemoveUnit(uint id);
    void ModifyUnit(uint id, Action<UnitState> modifier);
    void ClearIfSnapshot();
}