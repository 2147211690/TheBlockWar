using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games;

public interface IBuff
{
    BuffTypeEnum BuffType { get; }
    void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationResult result);
    void OnUnitBeforeDamage(IGameState state, IReadOnlyUnitState unit, float rawDamage,
        ref float resultDamage, SimulationResult result);
    void OnUnitAfterDamage(IGameState state, IReadOnlyUnitState unit, float getDamage, 
        SimulationResult result);
    void OnUnitBeforeCure(IGameState state, IReadOnlyUnitState unit, float rawCure,
        ref float resultCure, SimulationResult result);
    void OnUnitAfterCure(IGameState state, IReadOnlyUnitState unit, float getCure,
        SimulationResult result);
    void OnRemoveFromUnit(IGameState state, IReadOnlyUnitState unit,
        SimulationResult result);
    void OnDurationChanged(IGameState state, IReadOnlyUnitState unit,
        int oldDuration, int newDuration, SimulationResult result);
}
