using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public class 屹立Buff() : Buff(BuffNames.屹立)
{
    public override void OnUnitBeforePush(SimObject obj, IReadOnlyUnitState unit, int buffCount, DirMask dir, ref bool shouldPush)
    {
        shouldPush = false;
    }
}