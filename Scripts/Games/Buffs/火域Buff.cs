using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public class 火域Buff() : Buff(BuffNames.火域)
{
    public override void OnEnterTurn(SimObject obj, IReadOnlyUnitState unit, int buffCount, int turn)
    {
        obj.ApplyDamage(unit.Coords, 10);
    }
}