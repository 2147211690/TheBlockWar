using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games;

public interface IBuff
{
    BuffTypeEnum BuffType { get; }
    void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationResult result, out IReadOnlyUnitState? renewedUnit);
    void OnUnitBeforeDamage(IGameState state, IReadOnlyUnitState unit, float rawDamage,
        ref float resultDamage, SimulationResult result, out IReadOnlyUnitState? renewedUnit);
    void OnUnitAfterDamage(IGameState state, IReadOnlyUnitState unit, float getDamage, 
        SimulationResult result, out IReadOnlyUnitState? renewedUnit);
    void OnUnitBeforeCure(IGameState state, IReadOnlyUnitState unit, float rawCure,
        ref float resultCure, SimulationResult result, out IReadOnlyUnitState? renewedUnit);
    void OnUnitAfterCure(IGameState state, IReadOnlyUnitState unit, float getCure,
        SimulationResult result, out IReadOnlyUnitState? renewedUnit);
    void OnRemoveFromUnit(IGameState state, IReadOnlyUnitState unit,
        SimulationResult result, out IReadOnlyUnitState? renewedUnit);
    void OnDurationChanged(IGameState state, IReadOnlyUnitState unit,
        int oldDuration, int newDuration, SimulationResult result, out IReadOnlyUnitState? renewedUnit);
}
