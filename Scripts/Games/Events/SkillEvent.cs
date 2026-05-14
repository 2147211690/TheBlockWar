using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Events;

public sealed class SkillEvent(long id) : SchedulerEvent(id)
{
    public long SkillBindUnitId { get; set => SetField(ref field, value); }
    public string SkillName { get; set => SetField(ref field, value); } = string.Empty;
    public Vector2I?[]? SkillArgs
    {
        get => SkillRawSkillArgs;
        set
        {
            if (SkillRawSkillArgs == null)
            {
                if (value == null) return;
                SkillRawSkillArgs = (Vector2I?[])value.Clone();
                return;
            }
            if (value == null)
            {
                SkillRawSkillArgs = null;
                return;
            }
            var va = SkillRawSkillArgs!;
            if (va.Length != value.Length) SkillRawSkillArgs = (Vector2I?[])value.Clone();
            var changed = false;
            for (var i = 0; i < va.Length; i++)
                if (va[i] != value[i])
                {
                    va[i] = value[i];
                    changed = true;
                }
            if (changed) SkillRawSkillArgs = va;
        }
    }
    private Vector2I?[]? SkillRawSkillArgs { get;set => SetField(ref field, value, false); }

    public override void OnApply(SchedulerEvent root, SimObject obj)
    {
        var unit = obj.State.UnitMapState.GetUnit(SkillBindUnitId);
        if (unit.IsNull()) return;
        var useCoords = unit.Coords;
        SkillName.GetSkillInstance().SkillCore(obj, unit, useCoords, SkillArgs ?? []);
    }
    public override string ToString()
    {
        return $"{base.ToString()} -> SkillEvent 单位:{SkillBindUnitId} 技能:{SkillName} 参数:{string.Join(",", SkillArgs ?? [])}";
    }
}