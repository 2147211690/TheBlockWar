using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([],[])]
        public ApplyResetUnitResult ApplyResetUnit()
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyResetUnitResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyResetUnitResult(CurrentDeep, CurrentTeam));
                foreach (var unit in obj.State.UnitMapState.GetAllUnits())
                {
                    obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.CanUseSkill = CanUseSkillMask.All);
                }
                return r;
            }
        }
    }

    public static partial void ApplyResetUnitReplayCore(IGameState state, ApplyResetUnitResult result)
    {
        foreach (var unit in state.UnitMapState.GetAllUnits())
        {
            state.UnitMapState.ModifyUnit(unit.Id, u => u.CanUseSkill = CanUseSkillMask.All);
        }
    }

    public static partial void ApplyResetUnitViewDraw(IGameView view, ReadOnlySimObject sim, ApplyResetUnitResult result)
    {
    }

    public static partial GDTask ApplyResetUnitViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyResetUnitResult result)
    {
        return GDTask.CompletedTask;
    }
}