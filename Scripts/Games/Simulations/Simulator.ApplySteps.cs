using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplySteps(IEnumerable<ApplyStep> applys, int startDelay, int endDelay, StepsRunningMode runningMode, SimulationContext context)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                context.Steps?.AddStep(new StepsStep(CurrentDeep, startDelay, endDelay, runningMode));
                foreach (var apply in applys)
                {
                    apply.Apply(state, context);
                }
            }
        }
    }
}