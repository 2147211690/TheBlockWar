using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games;

public static class Skills
{
    private static readonly Func<Vector2I, int, Vector2I[], IEnumerable<Vector2I>> EmptyArgsFunc = (_, _, _) => [];
    private static void Skill1Core(
        IGameState gameState,
        SimulationResult result,
        UnitState unit,
        Vector2I[] args)
    {
        gameState.ApplyDamage(unit.Coords + Vector2I.Up, 10, result);
        gameState.ApplyDamage(unit.Coords + Vector2I.Left, 10, result);
        gameState.ApplyDamage(unit.Coords + Vector2I.Right, 10, result);
        gameState.ApplyDamage(unit.Coords + Vector2I.Down, 10, result);
    }
    public static ISkill Skill1 { get; }
    = new ActionSkill(0, EmptyArgsFunc, Skill1Core);



    public class ActionSkill : ISkill
    {
        private Func<Vector2I, int, Vector2I[], IEnumerable<Vector2I>> _getSkillArgs;
        private Action<IGameState, SimulationResult, UnitState, Vector2I[]> _skillCore;
        public ActionSkill(int argCount, Func<Vector2I, int, Vector2I[], IEnumerable<Vector2I>> getSkillArgs, Action<IGameState, SimulationResult, UnitState, Vector2I[]> skillCore)
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

        public void SkillCore(IGameState gameState, SimulationResult result, in UnitState unit, Vector2I[] args)
        {
            _skillCore(gameState, result, unit, args);
        }
    }
}