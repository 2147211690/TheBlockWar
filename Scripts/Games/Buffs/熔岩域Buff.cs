using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public class 熔岩域Buff() : Buff(BuffNames.熔岩域)
{
    public override void OnEnterTurn(SimObject obj, IReadOnlyUnitState unit, int turn)
    {
        obj.ApplyDamage(unit.Coords, 10);
    }
}