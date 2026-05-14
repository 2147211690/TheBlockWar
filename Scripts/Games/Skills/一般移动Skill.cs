using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Skills;

public class 一般移动Skill() : Skill(SkillNames.一般移动, 1)
{
    public override IEnumerable<Vector2I> GetSkillArgs(IReadOnlyGameState state, IReadOnlyUnitState caster, Vector2I useCoords, int argsIndex,
        Vector2I?[] args)
    {
        if (argsIndex == 0)
        {
            return GameNavigator
                .FindValidDestinationsWithinCost(state, caster, caster.Coords, caster.GetMovePower(state))
                .Select(a => a - caster.Coords);
        }
        return [];
    }

    protected override void Core(SimObject obj, IReadOnlyUnitState unit, Vector2I useCoords,
        Vector2I?[] args)
    {
        if (args.TryGetArgs(0, out var v1))
        {
            
            var (path, cost) = GameNavigator.FindShortestPath(obj.State, unit, useCoords, v1 + unit.Coords);
            if (cost <= unit.GetMovePower(obj.State)) obj.ApplyMoveUnitPath(Vector2I.Zero, path);
        }
    }
}