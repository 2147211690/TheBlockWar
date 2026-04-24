using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Buffs;

public interface IBuff
{
    BuffInfo Info { get; }
    void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationContext context);
    void OnUnitBeforeDamage(IGameState state, IReadOnlyUnitState unit, float rawDamage,
        ref float resultDamage, SimulationContext context);
    void OnUnitAfterDamage(IGameState state, IReadOnlyUnitState unit, float getDamage, 
        SimulationContext context);
    void OnUnitBeforeCure(IGameState state, IReadOnlyUnitState unit, float rawCure,
        ref float resultCure, SimulationContext context);
    void OnUnitAfterCure(IGameState state, IReadOnlyUnitState unit, float getCure,
        SimulationContext context);
    void OnRemoveFromUnit(IGameState state, IReadOnlyUnitState unit,
        SimulationContext context);
    void OnDurationChanged(IGameState state, IReadOnlyUnitState unit,
        int oldDuration, int newDuration, SimulationContext context);
    void OnEnterTurn(IGameState state, IReadOnlyUnitState unit, int turn, SimulationContext context);
}
