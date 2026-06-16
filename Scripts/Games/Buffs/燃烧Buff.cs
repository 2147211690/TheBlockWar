using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Buffs;

public class 燃烧Buff() : Buff(BuffNames.燃烧)
{
    public override void OnEnterTurn(SimObject obj, IReadOnlyUnitState unit, int buffCount, int turn)
    {
        obj.ApplyDamage(unit.Coords, buffCount * 10);
    }

    public override void OnShouldAddToUnit(IReadOnlyGameState state, IReadOnlyUnitState unit, ref bool shouldAdd)
    {
        if (unit.FindBuff(BuffNames.水域) != -1) shouldAdd = false;
    }
}