using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;
[RegisterChildClass]
public abstract class Buff(string name) : IBuff
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public string Name { get; } = name;

    public virtual void OnAddToUnit(SimObject obj, IReadOnlyUnitState unit, int buffCount)
    {
    }

    public virtual void OnUnitGetMovePower(IReadOnlyGameState state, IReadOnlyUnitState unit, int buffCount,
        int defaultValue,
        ref int value)
    {
    }

    public virtual void OnUnitBeforeGetDamage(SimObject obj, IReadOnlyUnitState unit, int buffCount, float originDamage,
        ref float resultDamage)
    {
    }

    public virtual void OnBeforeDamageToUnit(SimObject obj, IReadOnlyUnitState unit, int buffCount,
        IReadOnlyUnitState target,
        float originDamage, ref float resultDamage)
    {
    }

    public virtual void OnUnitAfterGetDamage(SimObject obj, IReadOnlyUnitState unit, int buffCount, float getDamage){}

    public virtual void OnUnitBeforeGetCure(SimObject obj, IReadOnlyUnitState unit, int buffCount, float originCure,
        ref float resultCure)
    {
    }

    public virtual void OnBeforeCureToUnit(SimObject obj, IReadOnlyUnitState unit, int buffCount, IReadOnlyUnitState target,
        float originCure, ref float resultCure)
    {
    }

    public virtual void OnUnitAfterGetCure(SimObject obj, IReadOnlyUnitState unit, int buffCount, float getCure)
    {
    }

    public virtual void OnRemoveFromUnit(SimObject obj, IReadOnlyUnitState unit, int buffCount)
    {
    }

    public virtual void OnBuffCountChanged(SimObject obj, IReadOnlyUnitState unit, int buffCount, int oldDuration, int newDuration)
    {
    }

    public virtual void OnUnitDeath(SimObject obj, IReadOnlyUnitState unit, int buffCount)
    {
    }

    public virtual void OnEnterTurn(SimObject obj, IReadOnlyUnitState unit, int buffCount, int turn)
    {
    }

    public virtual void OnUnitBeforePush(SimObject obj, IReadOnlyUnitState unit, int buffCount, DirMask dir, ref bool shouldPush)
    {
    }

    public virtual void OnUnitShouldUseSkill(IReadOnlyGameState state, IReadOnlyUnitState unit, int buffCount, ISkill skill,
        ref bool shouldUseSkill)
    {
    }
}