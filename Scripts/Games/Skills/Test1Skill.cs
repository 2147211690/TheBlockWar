using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Skills;

public class Test1Skill() : Skill(SkillNameEnum.Test1, 2)
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

    public override void SkillCore(IGameState gameState, SimulationContext context, IReadOnlyUnitState unit, Vector2I useCoords,
        Vector2I?[] args)
    {
        if (args.TryGetArgs(0, out var v1))
        {
            
            var (path, cost) = GameNavigator.FindShortestPath(gameState, unit, useCoords, v1 + unit.Coords);
            if (cost <= 4) gameState.ApplyMoveUnitPath(Vector2I.Zero, path, context);
        }
        if (args.TryGetArgs(1, out var v2) && context.PeekStep is MoveUnitPathStep moveStep)
        {
            var coords = v2 + useCoords;
            for (int i = 0; i < 4; i++)
            {
                var vec = (DirMask)(1 << i);
                if (i != 0) gameState.ApplyNextParallel(context);
                gameState.ApplyPushUnit(coords + vec.ToVec2I(), vec, context);
            }
        }
    }
}