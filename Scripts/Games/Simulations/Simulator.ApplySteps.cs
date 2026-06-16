using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([], [])]
        public ApplyStepsResult ApplySteps(List<ApplyStep>? applys, int startDelay, int endDelay, StepsRunningMode runningMode)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyStepsResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyStepsResult(
                    CurrentDeep, CurrentTeam, applys, startDelay, endDelay, runningMode));
                foreach (var apply in applys ?? [])
                {
                    apply.Apply(obj);
                }
                return r;
            }
        }
    }
    public static partial void ApplyStepsViewDraw(GameView view, ReadOnlySimObject sim, ApplyStepsResult result)
    {
    }
    
    public static partial GDTask ApplyStepsViewRun(GameView view, ReadOnlySimObject sim, ApplyStepsResult result)
    {
        return GDTask.CompletedTask;
    }
}