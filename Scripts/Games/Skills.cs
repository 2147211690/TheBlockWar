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
            //gameState.ApplyDamage(unit.Coords, 20, result);
            gameState.ApplyMoveUnit(unit.Coords, unit.Coords + v1, result);
            if (result.PeekStep is MoveUnitStep {ResultIsValid:true})
            {
                gameState.ApplyDamage(unit.Coords + v1, 20, result);
            }
        }
    }

    private static IEnumerable<Vector2I> Skill1Args(
        IReadOnlyGameState state,
        IReadOnlyUnitState caster,
        int argsIndex,
        Vector2I?[] args)
    {
        return GameNavigator
            .FindValidDestinationsWithinCost(state, caster, caster.Coords, 4)
            .Select(a => a - caster.Coords);
    }
    public static ISkill Skill1 { get; } = new ActionSkill(1, Skill1Args, Skill1Core);



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