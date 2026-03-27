using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games;

public class SkillSchedulerEvent : SchedulerEvent
{
    public SkillSchedulerEvent(long bindUnitId, ISkill? skill, int team)
    {
        BindUnitId = bindUnitId;
        Skill = skill;
        Team = team;
    }
    public SkillSchedulerEvent() {}
    public long BindUnitId { get; set => SetField(ref field, value); }
    public ISkill? Skill { get; set => SetField(ref field, value); }
    public Vector2I?[]? SkillArgs
    {
        get => RawSkillArgs;
        set
        {
            if (RawSkillArgs == null)
            {
                if (value == null) return;
                RawSkillArgs = (Vector2I?[])value.Clone();
                return;
            }
            if (value == null)
            {
                RawSkillArgs = null;
                return;
            }
            var va = RawSkillArgs!;
            if (va.Length != value.Length) RawSkillArgs = (Vector2I?[])value.Clone();
            var changed = false;
            for (var i = 0; i < va.Length; i++)
                if (va[i] != value[i])
                {
                    va[i] = value[i];
                    changed = true;
                }
            if (changed) RawSkillArgs = va;
        }
    }
    private Vector2I?[]? RawSkillArgs { get;set => SetField(ref field, value, false); }

    public override void OnApply(SchedulerEvent root, IGameState state, SimulationResult result)
    {
        var unit = state.UnitMapState.GetUnit(BindUnitId);
        if (unit.IsNull() || Skill == null) return;
        var useCoords = unit.Coords;
        Skill.SkillCore(state, result, unit, useCoords, SkillArgs ?? []);
    }
}