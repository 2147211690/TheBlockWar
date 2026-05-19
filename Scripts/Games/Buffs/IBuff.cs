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
    void OnAddToUnit(SimObject obj, IReadOnlyUnitState unit);
    void OnUnitGetMovePower(IReadOnlyGameState state, IReadOnlyUnitState unit,
        int defaultValue, ref int value);
    void OnUnitBeforeGetDamage(SimObject obj, IReadOnlyUnitState unit, float originDamage,
        ref float resultDamage);
    void OnBeforeDamageToUnit(SimObject obj, IReadOnlyUnitState unit, IReadOnlyUnitState target, float originDamage,
        ref float resultDamage);
    void OnUnitAfterGetDamage(SimObject obj, IReadOnlyUnitState unit, float getDamage);
    void OnUnitBeforeGetCure(SimObject obj, IReadOnlyUnitState unit, float originCure,
        ref float resultCure);
    void OnBeforeCureToUnit(SimObject obj, IReadOnlyUnitState unit, IReadOnlyUnitState target, float originCure,
        ref float resultCure);
    void OnUnitAfterGetCure(SimObject obj, IReadOnlyUnitState unit, float getCure);
    void OnRemoveFromUnit(SimObject obj, IReadOnlyUnitState unit);
    void OnDurationChanged(SimObject obj, IReadOnlyUnitState unit,
        int oldDuration, int newDuration);
    void OnUnitDeath(SimObject obj, IReadOnlyUnitState unit);
    void OnEnterTurn(SimObject obj, IReadOnlyUnitState unit, int turn);
    void OnUnitBeforePush(SimObject obj, IReadOnlyUnitState unit, DirMask dir, ref bool shouldPush);
    void OnUnitShouldUseSkill(IReadOnlyUnitState unit, ISkill skill, ref bool shouldUseSkill);

}
