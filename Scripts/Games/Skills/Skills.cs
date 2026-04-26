using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Skills;

public static class Skills
{
    private static readonly Dictionary<string, ISkill> _skills = new();
    public static void Register(ISkill skill)
    {
        if (_skills.TryAdd(skill.SkillName, skill)) return;
        GD.PushWarning($"{skill.GetType()}:另一个技能已注册为{skill.SkillName}");
    }
    public static ISkill GetSkillInstance(this string type)
    {
        if (_skills.TryGetValue(type, out var skill)) return skill;
        GD.PushWarning($"未注册技能:{type},使用空技能");
        _skills[type] = skill = new EmptySkill(type);
        return skill;
    }
}
public class EmptySkill(string skillName) : Skill(skillName, 0)
{
    public override IEnumerable<Vector2I> GetSkillArgs(IReadOnlyGameState state, IReadOnlyUnitState caster,
        Vector2I useCoords, int argsIndex,
        Vector2I?[] args) => [];

    public override void SkillCore(IGameState gameState, SimulationContext context, IReadOnlyUnitState unit,
        Vector2I useCoords,
        Vector2I?[] args){}
}
