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

    public virtual void OnAddToUnit(SimObject obj, IReadOnlyUnitState unit)
    {
    }
    public virtual void OnUnitBeforeSkill(IReadOnlyGameState state, IReadOnlyUnitState unit, string skillName,
        ref bool canUseSkill)
    {
    }
    
    public virtual void OnUnitBeforeGetDamage(SimObject obj, IReadOnlyUnitState unit, float originDamage,
        ref float resultDamage)
    {
    }

    public virtual void OnBeforeDamageToUnit(SimObject obj, IReadOnlyUnitState unit, IReadOnlyUnitState target,
        float originDamage, ref float resultDamage)
    {
    }

    public virtual void OnUnitAfterGetDamage(SimObject obj, IReadOnlyUnitState unit, float getDamage)
    {
    }

    public virtual void OnUnitBeforeGetCure(SimObject obj, IReadOnlyUnitState unit, float originCure,
        ref float resultCure)
    {
    }

    public virtual void OnBeforeCureToUnit(SimObject obj, IReadOnlyUnitState unit, IReadOnlyUnitState target,
        float originCure, ref float resultCure)
    {
    }

    public virtual void OnUnitAfterGetCure(SimObject obj, IReadOnlyUnitState unit, float getCure)
    {
    }

    public virtual void OnRemoveFromUnit(SimObject obj, IReadOnlyUnitState unit)
    {
    }

    public virtual void OnUnitDeath(SimObject obj, IReadOnlyUnitState unit)
    {
    }

    public virtual void OnDurationChanged(SimObject obj, IReadOnlyUnitState unit, int oldDuration, int newDuration)
    {
    }

    public virtual void OnEnterTurn(SimObject obj, IReadOnlyUnitState unit, int turn)
    {
    }
}