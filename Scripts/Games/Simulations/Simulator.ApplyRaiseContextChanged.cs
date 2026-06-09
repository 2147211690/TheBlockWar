using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([],[])]
        public void ApplyRaiseContextChanged(SimContext context)
        {
            EnterSimulation();
            obj.Steps?.AddResult(new ApplyRaiseContextChangedResult(CurrentDeep, context));
            ExitSimulation();
        }
    }
    public static partial void ApplyRaiseContextChangedViewDraw(GameView view, ReadOnlySimObject sim, ApplyRaiseContextChangedResult result)
    {
    }
    
    public static partial GDTask ApplyRaiseContextChangedViewRun(GameView view, ReadOnlySimObject sim, ApplyRaiseContextChangedResult result)
    {
        return GDTask.CompletedTask;
    }
}