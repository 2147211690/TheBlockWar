using System;
using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games;

public static class Skills
{
    private static readonly Func<Vector2I, int, Vector2I[], IEnumerable<Vector2I>> EmptyArgsFunc = (_, _, _) => [];
    private static void Skill1Core(
        IGameState gameState,
        SimulationResult result,
        IReadOnlyUnitState unit,
        Vector2I[] args)
    {
        if (args.TryGetArgs(0, out var v1))
        {
            gameState.ApplyMoveUnit(unit.Coords, unit.Coords + v1, result);
        }
    }
    public static ISkill Skill1 { get; }
    = new ActionSkill(1,(i, i1, arg3) => CoordsRegion.GetCircleRegion(Vector2I.Zero, 5) , Skill1Core);



    public class ActionSkill : ISkill
    {
        private Func<Vector2I, int, Vector2I[], IEnumerable<Vector2I>> _getSkillArgs;
        private Action<IGameState, SimulationResult, IReadOnlyUnitState, Vector2I[]> _skillCore;
        public ActionSkill(
            int argCount,
            Func<Vector2I, int, Vector2I[], IEnumerable<Vector2I>> getSkillArgs,
            Action<IGameState, SimulationResult, IReadOnlyUnitState, Vector2I[]> skillCore)
        {
            ArgCount = argCount;
            _getSkillArgs = getSkillArgs;
            _skillCore = skillCore;
        }
        public int ArgCount { get; }
        public IEnumerable<Vector2I> GetSkillArgs(Vector2I unitCoords, int argsIndex, Vector2I[] args)
        {
            return _getSkillArgs(unitCoords, argsIndex, args);
        }

        public void SkillCore(IGameState gameState, SimulationResult result, IReadOnlyUnitState unit, Vector2I[] args)
        {
            _skillCore(gameState, result, unit, args);
        }
    }
}