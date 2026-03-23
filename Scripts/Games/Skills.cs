using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games;

public static class Skills
{
    private static readonly ActionSkill.GetSkillArgsFunc EmptyArgsFunc = (_, _, _, _, _) => [];
    private static readonly ActionSkill.GetSkillArgsFunc SelfArgsFunc = (_, _, _, _, _) => [Vector2I.Zero];
    /// <summary>
    /// 会传入快照状态，需要在快照状态下进行操作
    /// </summary>
    private static void Skill1Core(
        IGameState gameState,
        SimulationResult result,
        IReadOnlyUnitState unit,
        Vector2I useCoords,
        Vector2I?[] args)
    {
        if (args.TryGetArgs(0, out var v1))
        {
            
            var path = GameNavigator.FindShortestPath(gameState, unit, useCoords, v1 + unit.Coords);
            gameState.ApplyMoveUnitPath(Vector2I.Zero, path, result);
        }
        if (args.TryGetArgs(1, out var v2) && result.PeekStep is MoveUnitPathStep moveStep)
        {
            var start = moveStep.Path[moveStep.ResultTargetIndex];
            var dir = (v2 - v1).ToVecDirMask();
            gameState.MoveHit(start, dir, 20, 4, !unit.IsFlying ,result);
            gameState.ApplyAnimation(new ThrowSomethingAnim(start, v2 + useCoords, "Test", 100, 1), result);
            gameState.ApplyModifyUnitBuff(v2 + useCoords, BuffTypeEnum.Test, BuffOperation.AddBuff, 1, result);
        }
        if (args.TryGetArgs(2, out var v3))
        {
            foreach(var offset in CoordsRegion.GetSquareRegion(Vector2I.Zero, 1))
            {
                gameState.ApplyModifyUnitBuff(v3 + useCoords + offset, BuffTypeEnum.Test2, BuffOperation.AddBuff, 1, result);
            }
        }
        if (args.TryGetArgs(3, out var v4))
        {
            gameState.ApplyModifyUnitBuff(v4 + useCoords, BuffTypeEnum.Test, BuffOperation.RemoveBuff, 1, result);
        }
    }
    /// <summary>
    /// 不会传入快照状态，直接操作原始状态
    /// </summary>
    private static IEnumerable<Vector2I> Skill1Args(
        IReadOnlyGameState state,
        IReadOnlyUnitState caster,
        Vector2I useCoords,
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
            return CoordsRegion.GetCrossRegion(args[0] ?? Vector2I.Zero, 4, 4, 4, 4);
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
    public static ISkill Skill1 { get; } = new ActionSkill(4, Skill1Args, Skill1Core);



    public class ActionSkill : ISkill
    {
        public delegate IEnumerable<Vector2I> GetSkillArgsFunc(
            IReadOnlyGameState state,
            IReadOnlyUnitState caster,
            Vector2I useCoords,
            int argsIndex,
            Vector2I?[] args);
        public delegate void SkillCoreFunc(
            IGameState gameState,
            SimulationResult result,
            IReadOnlyUnitState unit,
            Vector2I useCoords,
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
            Vector2I useCoords,
            int argsIndex,
            Vector2I?[] args)
        {
            return _getSkillArgs(state, caster, useCoords, argsIndex, args);
        }

        public void SkillCore(IGameState gameState, SimulationResult result, IReadOnlyUnitState unit, Vector2I useCoords, Vector2I?[] args)
        {
            _skillCore(gameState, result, unit, useCoords, args);
        }
    }
}