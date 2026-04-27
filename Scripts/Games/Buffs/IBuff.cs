using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Buffs;

public interface IBuff
{
    string Name { get; }
    void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationContext context);

    void OnUnitBeforeSkill(IGameState state, IReadOnlyUnitState unit, string skillName,
        ref bool canUseSkill, SimulationContext context);
    void OnUnitBeforeGetDamage(IGameState state, IReadOnlyUnitState unit, float originDamage,
        ref float resultDamage, SimulationContext context);
    void OnBeforeDamageToUnit(IGameState state, IReadOnlyUnitState unit, IReadOnlyUnitState target, float originDamage,
        ref float resultDamage, SimulationContext context);
    void OnUnitAfterGetDamage(IGameState state, IReadOnlyUnitState unit, float getDamage, 
        SimulationContext context);
    void OnUnitBeforeGetCure(IGameState state, IReadOnlyUnitState unit, float originCure,
        ref float resultCure, SimulationContext context);
    void OnBeforeCureToUnit(IGameState state, IReadOnlyUnitState unit, IReadOnlyUnitState target, float originCure,
        ref float resultCure, SimulationContext context);
    void OnUnitAfterGetCure(IGameState state, IReadOnlyUnitState unit, float getCure,
        SimulationContext context);
    void OnRemoveFromUnit(IGameState state, IReadOnlyUnitState unit,
        SimulationContext context);
    void OnDurationChanged(IGameState state, IReadOnlyUnitState unit,
        int oldDuration, int newDuration, SimulationContext context);
    void OnEnterTurn(IGameState state, IReadOnlyUnitState unit, int turn, SimulationContext context);
}
