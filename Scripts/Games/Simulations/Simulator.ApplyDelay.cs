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
            var r = obj.Steps.AddResult(new ApplyDelayResult(CurrentDeep, CurrentTeam, delay));
            ExitSimulation();
            return r;
        }
    }
    public static partial void ApplyDelayViewDraw(IGameView view, ReadOnlySimObject sim, ApplyDelayResult result)
    {
    }
    
    public static partial GDTask ApplyDelayViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyDelayResult result)
    {
        return GDTask.Delay(result.Delay);
    }
}