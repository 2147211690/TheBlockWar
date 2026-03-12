using Godot;

namespace TheBlockWar.Scripts.Games;

public class SkillEvent : SchedulerEvnet
{
    public new Vector2I[]? Args { get => base.Args as Vector2I[]; set => base.Args = value; }
    public uint BindUnitId { get => field; set => SetField(ref field, value); }
    public ISkill Skill { get; }

    public SkillEvent(uint id, ISkill skill) : base(id)
    {
        Skill = skill;
        Action = (s, a, r) =>
        {
            var unit = s.GetUnit(BindUnitId);
            if (unit == null) return;
            Skill.SkillCore(s, r, unit.Value, a as Vector2I[] ?? []);
        };
    }

    public override string ToString()
    {
        return $"{base.ToString()}-技能绑定id:{BindUnitId} 技能类型:{Skill.GetType()} 技能哈希:{Skill.GetHashCode()}";
        
    }
}
