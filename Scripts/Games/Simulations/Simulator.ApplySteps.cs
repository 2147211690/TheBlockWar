using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([], [])]
        public void ApplySteps(List<ApplyStep>? applys, int startDelay, int endDelay, StepsRunningMode runningMode)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                obj.Steps?.AddResult(new ApplyStepsResult(CurrentDeep, applys, startDelay, endDelay, runningMode));
                foreach (var apply in applys ?? [])
                {
                    apply.Apply(obj);
                }
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