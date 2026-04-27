using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Skills;
[RegisterChildClass(typeof(Skills))]
public abstract class Skill(string skillName, int argCount) : ISkill
{
    
    public string SkillName { get; } = skillName;
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
    public void SkillCore(IGameState gameState, SimulationContext context, IReadOnlyUnitState unit,
        Vector2I useCoords, Vector2I?[] args, string skillName)
    {
        if (!CheckCanUseSkill(gameState, context, unit, skillName)) return;
        Core(gameState, context, unit, useCoords, args, skillName);
    }
    
    protected abstract void Core(IGameState gameState, SimulationContext context, IReadOnlyUnitState unit, 
        Vector2I useCoords, Vector2I?[] args, string skillName);

    public bool CheckCanUseSkill(IGameState gameState, SimulationContext context, IReadOnlyUnitState unit, string skillName)
    {
        bool canUseSkill = true;
        foreach (var buffName in unit.GetInfoAndCurrentBuffs())
        {
            buffName.GetBuffInstance().OnUnitBeforeSkill(gameState, unit, skillName, ref canUseSkill, context);
        }
        return canUseSkill;
    }
}