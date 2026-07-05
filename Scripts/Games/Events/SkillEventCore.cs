using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Events;

public record SkillEventCore(string SkillName, long BindUnitId, Vector2I?[] Args) : EventCore
{
    public override long GetBindUnitId(EventState eventState)
    {
        return BindUnitId;
    }

    public override void OnApply(EventState eventState, SimObject obj)
    {
        var unit = obj.State.UnitMapState.GetUnit(BindUnitId);
        obj.ApplyUnitSkill(BindUnitId, SkillName, unit.Coords, Args);
    }

    public override bool CheckIsValid(EventState eventState, IReadOnlyGameState gameState)
    {
        var unit = gameState.UnitMapState.GetUnit(BindUnitId);
        return unit.GetShouldUseSkill(gameState, SkillName.GetSkillInfo().Instance);
    }
}