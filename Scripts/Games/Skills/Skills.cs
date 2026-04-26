using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Skills;

public static class Skills
{
    private static readonly ActionSkill.GetSkillArgsFunc EmptyArgsFunc = (_, _, _, _, _) => [];
    private static readonly ActionSkill.GetSkillArgsFunc SelfArgsFunc = (_, _, _, _, _) => [Vector2I.Zero];
    private static readonly Dictionary<SkillEnum, ISkill> _skills = new();
    public static void Register(ISkill skill)
    {
        _skills[skill.SkillEnum] = skill;
    }
    public static ISkill? GetInstance(this SkillEnum type)
    {
        return _skills.GetValueOrDefault(type);
    }

    static Skills()
    {
        Register(new ActionSkill(2, Skill1Args, Skill1Core, SkillEnum.Test1));
    }
    
    /// <summary>
    /// 会传入快照状态，需要在快照状态下进行操作
    /// </summary>
    private static void Skill1Core(
        IGameState gameState,
        SimulationContext context,
        IReadOnlyUnitState unit,
        Vector2I useCoords,
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
            SimulationContext context,
            IReadOnlyUnitState unit,
            Vector2I useCoords,
            Vector2I?[] args);
        
        private GetSkillArgsFunc _getSkillArgs;
        private SkillCoreFunc _skillCore;
        public ActionSkill(
            int argCount,
            GetSkillArgsFunc getSkillArgs,
            SkillCoreFunc skillCore, SkillEnum skillEnum)
        {
            ArgCount = argCount;
            _getSkillArgs = getSkillArgs;
            _skillCore = skillCore;
            SkillEnum = skillEnum;
        }

        public SkillEnum SkillEnum { get; }
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

        public void SkillCore(IGameState gameState, SimulationContext context, IReadOnlyUnitState unit, Vector2I useCoords, Vector2I?[] args)
        {
            _skillCore(gameState, context, unit, useCoords, args);
        }
    }
}