using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step(["Valid"],[typeof(bool)])]
        public ApplyUnitSkillResult ApplyUnitSkill(long unitId, string skillName, Vector2I useCoords, Vector2I?[]? args)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyUnitSkillResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyUnitSkillResult(
                    CurrentDeep, CurrentTeam, unitId, skillName, useCoords, args));
                var unit = obj.State.UnitMapState.GetUnit(unitId);
                var skill = skillName.GetSkillInfo().Instance;
                if (unit.IsNull() || !unit.GetShouldUseSkill(obj.State, skill))
                    return r;
                r.ReValid = true;
                obj.State.UnitMapState.ModifyUnit(unit.Id, 
                    u => u.CanUseSkill &= 
                        skillName.EndsWith("移动") ?
                            ~CanUseSkillMask.Move : 
                            ~CanUseSkillMask.All);
                skill.SkillCore(obj, unit, useCoords, args);
                return r;
            }
        }
    }

    public static partial void ApplyUnitSkillReplayCore(IGameState state, ApplyUnitSkillResult result)
    {
        if (!result.ReValid) return;
        state.UnitMapState.ModifyUnit(result.UnitId, 
            u => u.CanUseSkill &= 
                result.SkillName.EndsWith("移动") ?
                    ~CanUseSkillMask.Move : 
                    ~CanUseSkillMask.All);
    }

    public static partial void ApplyUnitSkillViewDraw(IGameView view, ReadOnlySimObject sim, ApplyUnitSkillResult result)
    {
    }

    public static partial GDTask ApplyUnitSkillViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyUnitSkillResult result)
    {
        return GDTask.CompletedTask;
    }
}