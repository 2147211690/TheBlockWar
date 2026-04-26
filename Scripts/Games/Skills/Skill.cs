using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Skills;
[RegisterChildClass(typeof(Skills))]
public abstract class Skill(SkillNameEnum skillEnum, int argCount) : ISkill
{
    
    public SkillNameEnum SkillEnum { get; } = skillEnum;
    public int ArgCount { get; } = argCount;

    /// <summary>
    /// 不会传入快照状态，直接操作原始状态
    /// </summary>
    public abstract IEnumerable<Vector2I> GetSkillArgs(IReadOnlyGameState state, IReadOnlyUnitState caster,
        Vector2I useCoords, int argsIndex,
        Vector2I?[] args);
    /// <summary>
    /// 会传入快照状态，需要在快照状态下进行操作
    /// </summary>
    public abstract void SkillCore(IGameState gameState, SimulationContext context, IReadOnlyUnitState unit,
        Vector2I useCoords,
        Vector2I?[] args);
}