using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Buffs;
[RegisterChildClass(typeof(Buffs))]
public abstract class Buff(BuffNameEnum name) : IBuff
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public BuffNameEnum Name { get; } = name;

    public virtual void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationContext context)
    {
    }

    public virtual void OnUnitBeforeDamage(IGameState state, IReadOnlyUnitState unit, float rawDamage,
        ref float resultDamage,
        SimulationContext context)
    {
    }

    public virtual void OnUnitAfterDamage(IGameState state, IReadOnlyUnitState unit, float getDamage,
        SimulationContext context)
    {
    }

    public virtual void OnUnitBeforeCure(IGameState state, IReadOnlyUnitState unit, float rawCure, ref float resultCure,
        SimulationContext context)
    {
    }

    public virtual void OnUnitAfterCure(IGameState state, IReadOnlyUnitState unit, float getCure,
        SimulationContext context)
    {
    }

    public virtual void OnRemoveFromUnit(IGameState state, IReadOnlyUnitState unit, SimulationContext context)
    {
    }

    public virtual void OnDurationChanged(IGameState state, IReadOnlyUnitState unit, int oldDuration, int newDuration,
        SimulationContext context)
    {
    }

    public virtual void OnEnterTurn(IGameState state, IReadOnlyUnitState unit, int turn, SimulationContext context)
    {
    }
}