using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public class 荆棘1Buff() : Buff(BuffNames.荆棘1)
{
    public override void OnUnitAfterGetDamage(SimObject obj, IReadOnlyUnitState unit, int buffCount, float getDamage)
    {
        var caster = obj.State.UnitMapState.GetUnit(Simulator.CurrentContext.SourceUnitId);
        if (caster.IsNull() || getDamage == 0) return;
        obj.PushContext(new SimContext(unit.Id));
        obj.ApplyModifyUnitBuff(unit.Coords, BuffNames.荆棘1, BuffOperation.RemoveBuff, 1, false);
        obj.ApplyDamage(caster.Coords, 10);
        obj.PopContext();
    }
}