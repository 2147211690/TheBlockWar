using Godot;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games;

public class SkillEvent : SchedulerEvent
{
    public new Vector2I?[]? Args
    {
        get => base.Args as Vector2I?[];
        set
        {
            if (base.Args == null)
            {
                if (value == null) return;
                base.Args = value.Clone();
                return;
            }
            if (value == null)
            {
                base.Args = null;
                return;
            }
            var va = (Vector2I?[])base.Args!;
            if (va.Length != value.Length) base.Args = value.Clone();
            var changed = false;
            for (var i = 0; i < va.Length; i++)
                if (va[i] != value[i])
                {
                    va[i] = value[i];
                    changed = true;
                }
            if (changed) base.Args = va;
        }
    }

    public uint BindUnitId { get => field; set => SetField(ref field, value); }
    public ISkill? Skill 
    { 
        get => field;
        set
        {
            field = value;
            Action = (s, a, r) =>
            {
                var unit = s.UnitMapState.GetUnit(BindUnitId);
                var useCoords = unit.Coords;
                if (unit.IsNull()) return;
                Skill?.SkillCore(s, r, unit, useCoords, a as Vector2I?[] ?? []);
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
