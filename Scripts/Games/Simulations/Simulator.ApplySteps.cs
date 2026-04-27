using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        public void ApplySteps(IEnumerable<ApplyStep> applys, int startDelay, int endDelay, StepsRunningMode runningMode)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                obj.Steps?.AddStep(new StepsStep(CurrentDeep, startDelay, endDelay, runningMode));
                foreach (var apply in applys)
                {
                    apply.Apply(obj);
                }
            }
        }
    }
}