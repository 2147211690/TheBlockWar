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
            gameState.ApplyModifyTileRegion(useCoords, RegionTypeEnum.Test1, RegionOperation.AddRegion, 1, result);
        }
        if (args.TryGetArgs(1, out var v2))
        {
            
            
            for (int i = 0; i < 4; i++)
            {
                var dir = (DirMask)(1 << i);
                gameState.ApplyPushUnit(v2 + useCoords + dir.ToVec2I(), dir, result);
            }
            gameState.ApplyDamage(useCoords + v2, 20, result);
        }
        if (args.TryGetArgs(2, out var v3))
        {
            var c = v3 + useCoords;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i == j && i == 0)
                    {
                        gameState.ApplyModifyUnitBuff(c + new Vector2I(i, j), BuffTypeEnum.Test2, BuffOperation.AddBuff, 1, result);
                    }
                    else gameState.ApplyCure(c + new Vector2I(i, j), 10, result);
                }
            }
            
        }
    }

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
            return CoordsRegion.GetCircleRegion(args[0] ?? Vector2I.Zero, 4);
            //.Where(a => caster.IsCanBeDestination(state ,a + caster.Coords));
        }
        else if(argsIndex == 2)
        {
            return CoordsRegion.GetCircleRegion(args[0] ?? Vector2I.Zero, 6);
            //.Where(a => caster.IsCanBeDestination(state ,a + caster.Coords));
        }
        return [];
    }
    public static ISkill Skill1 { get; } = new ActionSkill(3, Skill1Args, Skill1Core);



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