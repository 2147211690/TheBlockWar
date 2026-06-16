using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public interface IBuff
{
    string Name { get; }
    [BuffHook]
    void OnAddToUnit(SimObject obj, IReadOnlyUnitState unit, int buffCount);
    [BuffHook]
    void OnUnitGetMovePower(IReadOnlyGameState state, IReadOnlyUnitState unit,
        int buffCount,
        int defaultValue, ref int value);
    [BuffHook]
    void OnUnitBeforeGetDamage(SimObject obj, IReadOnlyUnitState unit, int buffCount, float originDamage,
        ref float resultDamage);
    [BuffHook]
    void OnBeforeDamageToUnit(SimObject obj, IReadOnlyUnitState unit, int buffCount, IReadOnlyUnitState target,
        float originDamage,
        ref float resultDamage);
    [BuffHook]
    void OnUnitAfterGetDamage(SimObject obj, IReadOnlyUnitState unit, int buffCount, float getDamage);
    [BuffHook]
    void OnUnitBeforeGetCure(SimObject obj, IReadOnlyUnitState unit, int buffCount, float originCure,
        ref float resultCure);
    [BuffHook]
    void OnBeforeCureToUnit(SimObject obj, IReadOnlyUnitState unit, int buffCount, IReadOnlyUnitState target, float originCure,
        ref float resultCure);
    [BuffHook]
    void OnUnitAfterGetCure(SimObject obj, IReadOnlyUnitState unit, int buffCount, float getCure);
    [BuffHook]
    void OnRemoveFromUnit(SimObject obj, IReadOnlyUnitState unit, int buffCount, int buffOldCount);
    [BuffHook]
    void OnBuffCountChanged(SimObject obj, IReadOnlyUnitState unit, int buffCount,
        int oldCount, int newCount);
    [BuffHook]
    void OnUnitDeath(SimObject obj, IReadOnlyUnitState unit, int buffCount);
    [BuffHook]
    void OnEnterTurn(SimObject obj, IReadOnlyUnitState unit, int buffCount, int turn);
    [BuffHook]
    void OnUnitBeforePush(SimObject obj, IReadOnlyUnitState unit, int buffCount, DirMask dir, ref bool shouldPush);
    [BuffHook]
    void OnUnitShouldUseSkill(IReadOnlyGameState state, IReadOnlyUnitState unit, int buffCount, ISkill skill, ref bool shouldUseSkill);
    [BuffHook]
    void OnShouldAddToUnit(IReadOnlyGameState state, IReadOnlyUnitState unit, ref bool shouldAdd);

}
