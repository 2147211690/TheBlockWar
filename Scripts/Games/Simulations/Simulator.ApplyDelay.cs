using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([], [])]
        public ApplyDelayResult ApplyDelay(int delay)
        {
            EnterSimulation(); 
            var r = obj.Steps.AddResult(new ApplyDelayResult(CurrentDeep, delay));
            ExitSimulation();
            return r;
        }
    }
    public static partial void ApplyDelayViewDraw(GameView view, ReadOnlySimObject sim, ApplyDelayResult result)
    {
    }
    
    public static partial GDTask ApplyDelayViewRun(GameView view, ReadOnlySimObject sim, ApplyDelayResult result)
    {
        return GDTask.Delay(result.Step.Delay);
    }
}