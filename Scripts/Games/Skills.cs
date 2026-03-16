using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games;

public static class Skills
{
    private static readonly ActionSkill.GetSkillArgsFunc EmptyArgsFunc = (_, _, _, _) => [];
    private static readonly ActionSkill.GetSkillArgsFunc SelfArgsFunc = (_, _, _, _) => [Vector2I.Zero];
    private static void Skill1Core(
        IGameState gameState,
        SimulationResult result,
        IReadOnlyUnitState unit,
        Vector2I?[] args)
    {
        if (args.TryGetArgs(0, out var v1))
        {
            var path = GameNavigator.FindShortestPath(gameState, unit, unit.Coords, v1 + unit.Coords);
            gameState.ApplyMoveUnitPath(Vector2I.Zero, path, result);
        }
        if (args.TryGetArgs(1, out var v2))
        {
            gameState.ApplyModifyUnitBuff(v2 + unit.Coords, BuffTypeEnum.Test, BuffOperation.AddBuff, 1, result);
            gameState.ApplyModifyUnitBuff(v2 + unit.Coords + Vector2I.Up, BuffTypeEnum.Test, BuffOperation.RemoveBuff, 1, result);
            for(int i = 0; i < 4; i++)
            {
                var dir = (DirMask)(1 << i);
                gameState.ApplyPushUnit(v2 + unit.Coords + dir.ToVec2I(), dir, result);
            }
        }
    }

    private static IEnumerable<Vector2I> Skill1Args(
        IReadOnlyGameState state,
        IReadOnlyUnitState caster,
        int argsIndex,
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
            return CoordsRegion.GetCircleRegion(args[0] ?? Vector2I.Zero, 3);
            //.Where(a => caster.IsCanBeDestination(state ,a + caster.Coords));
        }

        return [];
    }
    public static ISkill Skill1 { get; } = new ActionSkill(2, Skill1Args, Skill1Core);



    public class ActionSkill : ISkill
    {
        public delegate IEnumerable<Vector2I> GetSkillArgsFunc(
            IReadOnlyGameState state,
            IReadOnlyUnitState caster,
            int argsIndex,
            Vector2I?[] args);
        public delegate void SkillCoreFunc(
            IGameState gameState,
            SimulationResult result,
            IReadOnlyUnitState unit,
            Vector2I?[] args);
        
        private GetSkillArgsFunc _getSkillArgs;
        private SkillCoreFunc _skillCore;
        public ActionSkill(
            int argCount,
            GetSkillArgsFunc getSkillArgs,
            SkillCoreFunc skillCore)
        {
            ArgCount = argCount;
            _getSkillArgs = getSkillArgs;
            _skillCore = skillCore;
        }
        public int ArgCount { get; }
        public IEnumerable<Vector2I> GetSkillArgs(
            IReadOnlyGameState state,
            IReadOnlyUnitState caster,
            int argsIndex,
            Vector2I?[] args)
        {
            return _getSkillArgs(state, caster, argsIndex, args);
        }

        public void SkillCore(IGameState gameState, SimulationResult result, IReadOnlyUnitState unit, Vector2I?[] args)
        {
            _skillCore(gameState, result, unit, args);
        }
    }
}