using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyDelay(int delay, SimulationContext context)
        {
            EnterSimulation();
            if (context.Layer == SimulationLayer.Running)
                context.Steps?.AddStep(new DelayStep(IsCurrentChildSimulation, delay));
            ExitSimulation();
        }
    }
}