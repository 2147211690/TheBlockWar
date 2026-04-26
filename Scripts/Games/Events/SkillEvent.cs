using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Events;

public sealed class SkillEvent(long id) : SchedulerEvent(id)
{
    public long SkillBindUnitId { get; set => SetField(ref field, value); }
    public SkillEnum SkillEnum { get; set => SetField(ref field, value); }
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

    public override void OnApply(SchedulerEvent root, IGameState state, SimulationContext context)
    {
        var unit = state.UnitMapState.GetUnit(SkillBindUnitId);
        if (unit.IsNull()) return;
        var useCoords = unit.Coords;
        SkillEnum.GetInstance()?.SkillCore(state, context, unit, useCoords, SkillArgs ?? []);
    }
    public override string ToString()
    {
        return $"{base.ToString()} -> SkillEvent 单位:{SkillBindUnitId} 技能:{SkillEnum} 参数:{string.Join(",", SkillArgs ?? [])}";
    }
}