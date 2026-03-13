using Godot;

namespace TheBlockWar.Scripts.Games;

public class SkillEvent : SchedulerEvnet
{
    public new Vector2I[]? Args { get => base.Args as Vector2I[]; set => base.Args = value; }
    public uint BindUnitId { get => field; set => SetField(ref field, value); }
    public ISkill? Skill 
    { 
        get => field;
        set
        {
            field = value;
            Action = (s, a, r) =>
            {
                var unit = s.GetUnit(BindUnitId);
                if (unit == null) return;
                Skill?.SkillCore(s, r, unit.Value, a as Vector2I[] ?? []);
            };
        }
    }

    public SkillEvent(uint id, ISkill? skill = null) : base(id)
    {
        Skill = skill;
    }

    public override string ToString()
    {
        return $"{base.ToString()}-技能绑定id:{BindUnitId} 技能类型:{Skill.GetType()} 技能哈希:{Skill.GetHashCode()}";
        
    }
}
