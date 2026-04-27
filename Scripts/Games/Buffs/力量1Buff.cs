using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public class 力量1Buff() : Buff(BuffNames.力量1)
{
    public override void OnBeforeDamageToUnit(SimObject obj, IReadOnlyUnitState unit, IReadOnlyUnitState target,
        float originDamage,
        ref float resultDamage)
    {
        resultDamage += 10;
    }
}