using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Buffs;
[RegisterChildClass(typeof(Buffs))]
public abstract class Buff(string name) : IBuff
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public string Name { get; } = name;

    public virtual void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationContext context)
    {
    }
    public virtual void OnUnitBeforeSkill(IGameState state, IReadOnlyUnitState unit, string skillName, 
        ref bool canUseSkill,
        SimulationContext context)
    {
    }
    
    public virtual void OnUnitBeforeGetDamage(IGameState state, IReadOnlyUnitState unit, float originDamage,
        ref float resultDamage,
        SimulationContext context)
    {
    }

    public virtual void OnBeforeDamageToUnit(IGameState state, IReadOnlyUnitState unit, IReadOnlyUnitState target, float originDamage, ref float resultDamage,
        SimulationContext context)
    {
    }

    public virtual void OnUnitAfterGetDamage(IGameState state, IReadOnlyUnitState unit, float getDamage,
        SimulationContext context)
    {
    }

    public virtual void OnUnitBeforeGetCure(IGameState state, IReadOnlyUnitState unit, float originCure, ref float resultCure,
        SimulationContext context)
    {
    }

    public virtual void OnBeforeCureToUnit(IGameState state, IReadOnlyUnitState unit, IReadOnlyUnitState target, float originCure, ref float resultCure,
        SimulationContext context)
    {
    }

    public virtual void OnUnitAfterGetCure(IGameState state, IReadOnlyUnitState unit, float getCure,
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