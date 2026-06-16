using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public class 贤者的庇护光环Buff() : Buff(BuffNames.贤者的庇护光环)
{
    public override void OnUnitBeforeGetDamage(SimObject obj, IReadOnlyUnitState unit, int buffCount, float originDamage,
        ref float resultDamage)
    {
        resultDamage = 0;
    }
}