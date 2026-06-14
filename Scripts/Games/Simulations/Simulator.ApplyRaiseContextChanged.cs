using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([],[])]
        public ApplyRaiseContextChangedResult ApplyRaiseContextChanged(SimContext context)
        {
            EnterSimulation();
            var r = obj.Steps.AddResult(new ApplyRaiseContextChangedResult(CurrentDeep, context));
            ExitSimulation();
            return r;
        }
    }
    public static partial void ApplyRaiseContextChangedViewDraw(GameView view, ReadOnlySimObject sim, ApplyRaiseContextChangedResult result)
    {
        var team = sim.State.UnitMapState.GetUnit(result.Step.Context.SourceUnitId).Team;
        view.SetCurrentTheme(view.GetTeamType(team));
    }
    
    public static partial GDTask ApplyRaiseContextChangedViewRun(GameView view, ReadOnlySimObject sim, ApplyRaiseContextChangedResult result)
    {
        var team = sim.State.UnitMapState.GetUnit(result.Step.Context.SourceUnitId).Team;
        view.SetCurrentTheme(view.GetTeamType(team));
        return GDTask.CompletedTask;
    }
}