using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public class 不稳定Buff() : Buff(BuffNames.不稳定)
{
    public override void OnUnitDeath(SimObject obj, IReadOnlyUnitState unit, int buffCount)
    {
        obj.PushContext(new SimContext(unit.Id));
        var coords = unit.Coords;
        obj.ApplyModifyUnitBuff(unit.Coords, BuffNames.不稳定, BuffOperation.RemoveBuff, int.MaxValue, false);
        obj.BeginSteps(0, 0, StepsRunningMode.ParallelWaitAll);
        for (var i = 0; i < 4; i++)
        {
            var dir = (DirMask)(1 << i);
            obj.ApplyPushUnit(coords + dir.ToVec2I(), dir);
        }
        obj.EndSteps();
        obj.PopContext();
    }
}