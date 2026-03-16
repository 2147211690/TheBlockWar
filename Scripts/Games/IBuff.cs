using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games;

public interface IBuff
{
    BuffTypeEnum BuffType { get; }
    void OnAddToUnit(IGameState state, uint id, SimulationResult result);
    void OnUnitBeforeDamage(IGameState state, uint id, float rawDamage, ref float resultDamage, SimulationResult result);
    void OnUnitAfterDamage(IGameState state, uint id, float getDamage, SimulationResult result);
    void OnRemoveFromUnit(IGameState state, uint id, SimulationResult result);
}