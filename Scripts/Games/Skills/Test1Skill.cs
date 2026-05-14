using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Skills;

public class Test1Skill() : Skill(SkillNames.Test1, 3)
{
    public override IEnumerable<Vector2I> GetSkillArgs(IReadOnlyGameState state, IReadOnlyUnitState caster, Vector2I useCoords, int argsIndex,
        Vector2I?[] args)
    {
        if (argsIndex == 0)
        {
            return GameNavigator
                .FindValidDestinationsWithinCost(state, caster, caster.Coords, 4)
                .Select(a => a - caster.Coords);
        }
        else if(argsIndex == 1)
        {
            return CoordsRegion.GetCircleRegion(args[0] ?? Vector2I.Zero, 6);
            //.Where(a => caster.IsCanBeDestination(state ,a + caster.Coords));
        }
        else if(argsIndex == 2)
        {
            return CoordsRegion.GetCircleRegion(args[0] ?? Vector2I.Zero, 6);
            //.Where(a => caster.IsCanBeDestination(state ,a + caster.Coords));
        }
        else if(argsIndex == 3)
        {
            return CoordsRegion.GetCircleRegion(args[0] ?? Vector2I.Zero, 6);
            //.Where(a => caster.IsCanBeDestination(state ,a + caster.Coords));
        }
        return [];
    }

    protected override void Core(SimObject obj, IReadOnlyUnitState unit, Vector2I useCoords,
        Vector2I?[] args)
    {
        if (args.TryGetArgs(0, out var v1))
        {
            obj.ApplyDamage(unit.Coords, 10);
        }
        if (args.TryGetArgs(1, out var v2) && obj.Steps?.PeekStep is MoveUnitPathStep moveStep)
        {
            var coords = v2 + useCoords;
            obj.BeginSteps(0, 0, StepsRunningMode.ParallelWaitAll);
            for (int i = 0; i < 4; i++)
            {
                var vec = (DirMask)(1 << i);
                obj.ApplyPushUnit(coords + vec.ToVec2I(), vec);
            }
            obj.EndSteps();
        }
        if (args.TryGetArgs(2, out var v3))
        {
            var coords = v3 + useCoords;
            obj.ApplyDamage(coords, 10);
        }
    }
}